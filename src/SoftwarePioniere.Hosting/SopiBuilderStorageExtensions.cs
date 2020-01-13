using System;
using Foundatio.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderStorageExtensions
    {
        public static ISopiBuilder AddStorage(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Log($"Storage Config Value: {builder.Options.Storage}");
            switch (builder.Options.Storage)
            {
                case SopiOptions.StorageFolder:
                    builder.Log("Adding Folder Storage");
                    builder.AddFolderStorage(c => config.Bind("FolderStorage", c));
                    break;

                case SopiOptions.StorageAzureStorage:
                    builder.Log("Adding Azure Storage");
                    builder.AddAzureStorage(c => config.Bind("AzureStorage", c));
                    break;
                default:
                    builder.Log("Adding InMemory Storage");
                    builder.AddInMemoryStorage();
                    break;
            }

            return builder;
        }


        public static ISopiBuilder AddInMemoryStorage(this ISopiBuilder builder)
        {
            builder.Services.AddSingleton<IFileStorage>(p =>

                new InMemoryFileStorage(optionsBuilder => optionsBuilder
                    .LoggerFactory(p.GetRequiredService<ILoggerFactory>()))
            );

            return builder;
        }

        public static ISopiBuilder AddFolderStorage(this ISopiBuilder builder, Action<FolderFileStorageOptions> configureOptions)
        {

            var opt = new FolderFileStorageOptions();
            configureOptions.Invoke(opt);

            builder.Services.AddSingleton<IFileStorage>(p =>



                new FolderFileStorage(optionsBuilder => optionsBuilder
                    .LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                    .Folder(opt.Folder))
            );

            return builder;
        }

        public static ISopiBuilder AddAzureStorage(this ISopiBuilder builder, Action<AzureFileStorageOptions> configureOptions)
        {
            var opt = new AzureFileStorageOptions();
            configureOptions.Invoke(opt);

            builder.Services.AddSingleton<IFileStorage>(p =>

                new AzureFileStorage(optionsBuilder => optionsBuilder
                    .LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                    .ConnectionString(opt.ConnectionString)
                    .ContainerName(opt.ContainerName)
                )
            );

            //builder.GetHealthChecksBuilder()
            //    .AddAzureBlobStorage(opt.ConnectionString, "azure-storage");

            return builder;
        }
    }
}
