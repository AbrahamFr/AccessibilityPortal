# DEV: Build in Debug Mode
# QA: Copy from bin\Debug to "Stage"
#Copy-Item -Path C:\dev\ComplianceSuite\BasicEnvironment\BasicEnvironmentPrereq\bin\Debug\netcoreapp2.1\* -Destination C:\dev\stage\BasicEnvironment\PrerequisiteChecker -recurse -Force
Copy-Item -Path C:\dev\ComplianceSuite\BasicEnvironment\BasicEnvironmentSetup\bin\Debug\* -Destination C:\dev\stage\Debug\BasicEnvironment\BasicEnvironmentSetup -recurse -Force

# QA: Zip BasicEnvironmentSetup and move both to "Publish
#Copy-Item -Path C:\dev\stage\BasicEnvironment\PrerequisiteChecker -Destination C:\dev\publish\BasicEnvironment\PrerequisiteChecker -recurse -Force
Compress-Archive -Path C:\dev\stage\Debug\BasicEnvironment\BasicEnvironmentSetup\* -CompressionLevel Fastest -DestinationPath C:\dev\ComplianceSuite\BasicEnvironment\BasicEnvironmentPrereq\bin\Debug\BasicEnvironmentSetup -Force

