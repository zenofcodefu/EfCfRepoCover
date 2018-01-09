using System;
using EfCfRepoCoverLib.CustomErrors;
using EfCfRepoCoverLib.DbConfigurations;

/// <summary>Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.</summary>
public static class ModuleInitializer
{
    /// <summary>Initializes the module.</summary>
    public static void Initialize()
    {
        try
        {
            DbConfigurationUtility.InitializeEfDbConfiguration(); // Call method to set current 'DbConfiguration' (e.g. 'MsSqlServerEfCfDbConfiguration', 'MySqlEfCfDbConfiguration', etc.).
        }
        catch (ConfigurationMissingException configurationMissingException)
        {
            // If this error is occurring, the required <appSettings> child element is missing (e.g. <add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" />).
            System.Diagnostics.Debug.WriteLine(configurationMissingException.ToString());
            throw;
        }
        catch (DbProviderTypeMismatchException dbProviderTypeMismatchException)
        {
            // If you're getting this error, make sure the <appSettings> element specified database type (e.g. 'MySql') matches the 'ProviderName=' database type in the connection string being used.
            // (e.g. '<add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" /> but 'ProviderName="System.Data.SQLite"' is specified in connection string)?
            System.Diagnostics.Debug.WriteLine(dbProviderTypeMismatchException.ToString());
            throw;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
            throw;
        }
    }
}