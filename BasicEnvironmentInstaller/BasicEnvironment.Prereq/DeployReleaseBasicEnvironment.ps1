# DEV: Build in Release Mode
# QA: Copy from bin\Release to "Stage"
Copy-Item -Path C:\dev\ComplianceSuite\BasicEnvironment\BasicEnvironmentPrereq\bin\Release\netcoreapp2.1\* -Destination C:\dev\stage\BasicEnvironment\PrerequisiteChecker -recurse -Force
Copy-Item -Path C:\dev\ComplianceSuite\BasicEnvironment\BasicEnvironmentSetup\bin\Release\* -Destination C:\dev\stage\BasicEnvironment\BasicEnvironmentSetup -recurse -Force

# QA: Zip BasicEnvironmentSetup and move both to "Publish
Copy-Item -Path C:\dev\stage\BasicEnvironment\PrerequisiteChecker -Destination C:\dev\publish\BasicEnvironment\ -recurse -Force
Compress-Archive -Path C:\dev\stage\BasicEnvironment\BasicEnvironmentSetup\* -CompressionLevel Fastest -DestinationPath C:\dev\publish\BasicEnvironment\BasicEnvironmentSetup -Force


# Support: Zip Basic Environment to Downloads folder
Compress-Archive -Path C:\dev\publish\BasicEnvironment\* -CompressionLevel Fastest -DestinationPath C:\Users\Administrator\Downloads\CsBasicEnvironment -Force


# Customer: Unzip from Downloads to Documents
Expand-Archive -LiteralPath C:\Users\Administrator\Downloads\CsBasicEnvironment.Zip -DestinationPath C:\Users\Administrator\Documents\ComplianceSheriff\CsBasicEnvironment -Force

# Customer: Click on Setup.bat
# Copy-Item -Path C:\dev\publish\BasicEnvironment\PrerequisiteChecker\* -Destination C:\Users\Administrator\Documents\ComplianceSheriff\CsBasicEnvironment -recurse -Force