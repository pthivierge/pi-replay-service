# CS-Solution-Template

This is a full C# Solution Template With Service, Command Line and settings editor.

This solution is a quick start, you can delete parts you dont need, for instance the service. It uses a shared config file for the solution so the .Settings Project can work for all projects in the solution.
Also there are post build events that puts everything togheter.  This is where you should test your application.

More explanations to come. Feel free to ask questions.


#Getting started

- Download the zip and extract it in a folder
- Open RenameProject.ps1 and CHANGE THE VARIABLES VALUES TO FIT YOUR NEED --> AKA: Company Name, Project/product long Name, Short name (used for namespaces), Service displayname and description, year.
- Right click RenameProject.ps1, and select "Run with powershell"
- Delete RenameProject.ps1
- Delete the .git folder (otherwise you'll keep tracking changes of the original repo)


- You may want to change icons and look of the Settings GUI


#How to use the template solution: 

+ The business code logic needs to be written in the Core dll.

+ Then use the command line and service code to write the code to call business code from the dll.

+ After compiling: always use the /Build folder in the first level directory level to test the application.

+ To install the service
 - run a command prompt as administrator
 - navigate to service folder
 - run: YourAppServiceService --install / YourAppServiceService --uninstall

#License
 
    Copyright 2015 Patrice Thivierge Fortin
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
