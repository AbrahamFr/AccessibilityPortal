using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Security.Cryptography;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class ClusterSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateClusterSettings(migrationBuilder);
            var jwTSigningKey = GenerateSecretKey();
            PopulateClusterSettings(migrationBuilder, jwTSigningKey);
        }

        private static void CreateClusterSettings(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE [dbo].[ClusterSettings](
	                                [ID] [int] IDENTITY(1,1) NOT NULL,
	                                [SettingsKey] [varchar](50) NOT NULL,
	                                [SettingsValue] [varchar](50) NOT NULL,
                                 CONSTRAINT [PK_ClusterSettings] PRIMARY KEY CLUSTERED 
                                (
	                                [ID] ASC
                                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                ) ON [PRIMARY]");
        }

        private static string GenerateSecretKey()
        {
            byte[] randomNumber = new byte[24];
            var rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber).Trim();
        }

        private static void PopulateClusterSettings(MigrationBuilder migrationBuilder, string jwTSigningKey)
        {
            migrationBuilder.Sql(@"SET IDENTITY_INSERT[dbo].[ClusterSettings] ON");
            migrationBuilder.Sql($@"INSERT[dbo].[ClusterSettings]
                                           ([ID],    [SettingsKey],    [SettingsValue]) 
                                     VALUES(   1, N'JwtSigningKey', N'{jwTSigningKey}')
                                 ");
            migrationBuilder.Sql($@"INSERT[dbo].[ClusterSettings]
                                           ([ID],    [SettingsKey],    [SettingsValue]) 
                                     VALUES(   2, N'JwtExpirationInMinutes', N'5')
                                 ");
            migrationBuilder.Sql(@"SET IDENTITY_INSERT[dbo].[ClusterSettings] OFF");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Drop TABLE [dbo].[ClusterSettings]");
        }
    }
}
