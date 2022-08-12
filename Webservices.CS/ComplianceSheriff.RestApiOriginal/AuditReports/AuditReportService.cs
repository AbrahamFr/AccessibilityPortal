using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Configuration;
using System.Text.RegularExpressions;

namespace ComplianceSheriff.AuditReports
{
    public static class AuditReportService
    {
        public static ValidationResponse ValidateUploadFormInputs(FormValueProvider formValueProvider, string fileNameFromQueryString)
        {
            var response = new ValidationResponse();

            //retrieve form values
            var frmValReportName = formValueProvider.GetValue("reportName").ToString().Trim();
            var frmValReportType = formValueProvider.GetValue("reportType").ToString().Trim();
            var uploadFileName = formValueProvider.GetValue("uploadFileName").ToString().Trim();

            //Validate Filename passed in from QueryString with Upload FileName in Request object
            if(!string.Equals(fileNameFromQueryString, uploadFileName, StringComparison.OrdinalIgnoreCase))
            {
                response.ApiResponse.ErrorCode = "api:auditReport:upload:fileNamesNotMatched";
                response.ApiResponse.StatusCode = 400;
                response.LogErrorMessage = $"File name from query string '{fileNameFromQueryString}' does not match the filename '{uploadFileName}' passed in via file input type";
                response.Reason = "unmatched Upload fileNames.";
                response.IsValid = false;

                return response;
            }

            //Validate ReportName is proper length
            if (String.IsNullOrWhiteSpace(frmValReportName) || (frmValReportName.Length < 5 || frmValReportName.Length > 100))
            {
                var reportNameLength = !String.IsNullOrWhiteSpace(frmValReportName) ? frmValReportName.Length : 0;

                response.ApiResponse.ErrorCode = "api:auditReport:upload:reportNameNotProperLength";
                response.ApiResponse.StatusCode = 400;
                response.LogErrorMessage = $"ReportName '{frmValReportName}' is not the proper length: {frmValReportName.Length}";
                response.Reason = "invalid ReportName length.";
                response.IsValid = false;

                return response;
            }

            //Validate supported file extensions
            var fileNameFromQueryStringExtension = Path.GetExtension(fileNameFromQueryString);
            var uploadFileNameExtension = Path.GetExtension(uploadFileName);
            if (!String.IsNullOrWhiteSpace(fileNameFromQueryStringExtension) && !String.IsNullOrWhiteSpace(uploadFileNameExtension))
            {
                if (!AuditReportService.GetMimeTypes().ContainsKey(uploadFileNameExtension))
                {
                    response.ApiResponse.ErrorCode = "api:auditReport:upload:unsupportedFileExtension";
                    response.ApiResponse.StatusCode = 400;
                    response.LogErrorMessage = $"File type '{uploadFileNameExtension}' is not a supported file type to be uploaded.";
                    response.Reason = "unsupported file extension.";
                    response.IsValid = false;

                    return response;
                }
            } else
            {
                response.ApiResponse.ErrorCode = "api:auditReport:upload:invalidFileExtension";
                response.ApiResponse.StatusCode = 400;
                response.LogErrorMessage = $"Empty File extension '{uploadFileNameExtension}' is not a supported file type to be uploaded.";
                response.Reason = "empty file extension.";
                response.IsValid = false;

                return response;
            }

            //Is ReportType a Number
            if (!Regex.IsMatch(frmValReportType, @"^\d+$"))
            {
                response.ApiResponse.ErrorCode = "api:auditReport:upload:invalidReportType";
                response.ApiResponse.StatusCode = 400;
                response.Reason = "ReportType is not a number.";
                response.LogErrorMessage = $"Invalid value passed into AuditReport Upload for reportType: {frmValReportType}.";
                response.IsValid = false;

                return response;

            } else {

                //Is ReportTypeId Between 1 and 4
                var reportTypeValue = Convert.ToInt32(frmValReportType);
                if (!(reportTypeValue > 0 && reportTypeValue < 5))
                {
                    response.ApiResponse.ErrorCode = "api:auditReport:upload:invalidReportType";
                    response.ApiResponse.StatusCode = 400;                    
                    response.Reason = "ReportType is not between 1 and 4";                    
                    response.LogErrorMessage = $"Invalid value passed into AuditReport Upload for reportType: {reportTypeValue}.";
                    response.IsValid = false;

                    return response;
                }
            }

            return response; 
        }

        public static string BuildAuditFilePath(HttpContext context, string fileName, ConfigurationOptions configOptions, JwtSignInHandler jwtSignInHandler)
        {
            string organizationId = String.Empty;

            var jwtPayloadHandler = new JwtPayloadHandler(jwtSignInHandler);
            var jwtPayLoad = jwtPayloadHandler.GetJwtPayload(context);

            organizationId = jwtPayLoad["organizationId"].ToString();

            //Build FilePath
            var filePath = Path.Combine("\\\\", configOptions.SharedDir, "Cryptzone", configOptions.ClusterName, "customers", organizationId, "Audit");

            return Path.Combine(filePath, fileName);
        }

        public static AuditReport BuildNewAuditReportRecord(string fullFilePath, FormValueProvider formValueProvider)
        {
            var fileInfo = new FileInfo(fullFilePath);
            var auditReportRecord = new AuditReport
            {
                ReportName = formValueProvider.GetValue("reportName").ToString().Trim(),
                ReportDescription = formValueProvider.GetValue("reportDescription").ToString(),
                AuditTypeId = Convert.ToInt32(formValueProvider.GetValue("reportType").ToString()),
                FileSize = fileInfo.Length / 1024,
                FileType = fileInfo.Extension,
                FileLocation = fileInfo.FullName,
                FileUploadDate = DateTime.UtcNow,
                FileStatusId = 1,
                LastModifiedDate = DateTime.UtcNow
            };
            return auditReportRecord;
        }

        public static string GetContentType(string path)
        {
            var types = AuditReportService.GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".config",  "application/xhtml+xml" },
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},                
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},  
                {".png", "image/png"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".js", "text/javascript" },
                {".rar", "application/x-rar-compressed" },
                {".tar", "application/x-tar" },
                {".tif", "image/tiff"},
                {".tiff", "image/tiff"},
                {".7z",  "application/x-7z-compressed" },
                {".xml",  "application/xml" },
                {".zip",  "application/zip" },
            };
        }
    }
}
