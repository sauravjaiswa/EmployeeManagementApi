using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Authentication;
using EmployeeManagementApi.Business;
using EmployeeManagementApi.Models;
using EmployeeManagementApi.Models.EmployeeDto;
using EmployeeManagementApi.Repositories.MongoDb;
using FluentValidation.AspNetCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmployeeManagementApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));

            services.AddControllers(configure =>
                {
                    configure.Filters.Add(new ProducesResponseTypeAttribute(500));
                    configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), 400));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddFluentValidation(config => config.RegisterValidatorsFromAssembly(typeof(Startup).Assembly));

            services.AddSwaggerGen();
            services.AddSwaggerGenNewtonsoftSupport();

            var mongoDbSettings = Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoDbSettings.ConnectionString);
            mongoClientSettings.SslSettings = new SslSettings {EnabledSslProtocols = SslProtocols.Tls12};

            var mongoClient = new MongoClient(mongoClientSettings);
            SetupMongoClientSettings();
            SetupMongoDb(mongoClient, mongoDbSettings);
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbSettings"));

            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddSingleton<IMongoDocumentRepositoryFactory, MongoDocumentRepositoryFactory>();

            services.AddSingleton<IEmployeeBal, EmployeeBal>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmployeeManagementApi");
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void SetupMongoDb(IMongoClient mongoClient, MongoDbSettings mongoDbSettings)
        {
            var db = mongoClient.GetDatabase(mongoDbSettings.DatabaseId);

            //SetCosmosDbThroughputIfNeeded(db, mongoDbSettings);   //Cannot implement this feature due to restrictions of pricing plan used.
            SetupCollectionIndexes(db, mongoDbSettings);
        }

        private static void SetCosmosDbThroughputIfNeeded(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {
            try
            {
                var result = db.RunCommand<BsonDocument>(new BsonDocument { { "customAction", "GetDatabase" } });

                var throughputIsNotSetToConfiguredValue =
                    !result.TryGetValue("autoScaleSettings", out var autoScaleSettings) ||
                    autoScaleSettings.AsBsonDocument.GetValue("maxThroughput")
                                     .AsInt32 != mongoDbSettings.CosmosDbAutoScaleThroughput;

                if (throughputIsNotSetToConfiguredValue)
                {
                    db.RunCommand<BsonDocument>(new BsonDocument
                    {
                        {"customAction", "UpdateDatabase"},
                        {
                            "autoScaleSettings", new BsonDocument
                            {
                                {"maxThroughput", mongoDbSettings.CosmosDbAutoScaleThroughput}
                            }
                        }
                    });
                }
            }
            catch (MongoCommandException e)
            {
                const int commandNotFoundErrorCode = 59;
                var isDatabaseException = e.Command.GetValue("customAction").AsString == "GetDatabase" ||
                                          e.Command.GetValue("customAction").AsString == "UpdateDatabase";

                if (e.Code == commandNotFoundErrorCode)
                {
                    // Noop, assume not using CosmosDB i.e. local development.
                }
                else if (isDatabaseException)
                {
                    db.RunCommand<BsonDocument>(new BsonDocument
                    {
                        {"customAction", "CreateDatabase"},
                        {
                            "autoScaleSettings", new BsonDocument
                            {
                                {"maxThroughput", mongoDbSettings.CosmosDbAutoScaleThroughput}
                            }
                        }
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        private static void SetupCollectionIndexes(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {
            CreateWildcardIndexForCollection<Employee>(db, mongoDbSettings);
            
            CreateTtlIndexForCollection<Employee>(db, mongoDbSettings);
        }

        private static void CreateWildcardIndexForCollection<T>(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {
            var collection = db.GetCollection<T>(mongoDbSettings.Collections[typeof(T).Name]
                                                                .CollectionId);

            collection.Indexes.CreateOne(new CreateIndexModel<T>(Builders<T>.IndexKeys.Wildcard()));
        }

        private static void CreateTtlIndexForCollection<T>(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {
            var collection = db.GetCollection<T>(mongoDbSettings.Collections[typeof(T).Name]
                                                                .CollectionId);
            // Cosmos currently only supports TTL indexes on Cosmos  internal modification timestamp field _ts.
            // _ts is a reserved property that contains the timestamp of the document's last modification.
            collection.Indexes.CreateOne(new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending("_ts"),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(mongoDbSettings.TtlInDays) }));
        }

        private static void SetupMongoClientSettings()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            ConventionRegistry.Register("Global Conventions",
                new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new EnumRepresentationConvention(BsonType.String),
                    new IgnoreExtraElementsConvention(true)
                },
                type => true);

            //BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
        }
    }
}
