# ManagementPortal.Ng

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 1.6.7.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build:prd` to build the project. The build artifacts will be stored in the `../ManagementPortal/wwwroot/` directory. Use the `-prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).

## System Requirements 
- Node.js v12
- npm v6

## Babel Transpilation of fizz-chart.js library file
## These instructions are for Local file generation that will be added to /src/assets/js folder for 
## chart library
- Follow babel docs for latest usage instructions https://babeljs.io/docs/en/usage
- All Babel modules are published as separate npm packages scoped under @babel
- Install the following core modules locally: @babel/core, @babel/cli, @babel/preset-env, @babel/polyfill
- Install the following plugins to transform ES2015 syntax to ES5: @babel/plugin-transform-arrow-functions, @babel/plugin-transform-runtime, @babel/plugin-transform-regenerator, @babel/runtime
- Run the following command from the `/CS.UI` root, on the locally downloaded fizz-chart.js library file:
`./node_modules/.bin/babel ~/Downloads/fizz-charts.js --out-dir . --plugins=@babel/plugin-transform-arrow-functions,@babel/plugin-transform-runtime --presets=@babel/env`
- Move the transpiled file into the `/src/assets/js` folder.  the `angular.json` file is configured to look in this directory for 3rd party JS files.
- Run `npm run build:prd` from `/CS.UI` folder to confirm successful build.  Test in Trend UI
- Commit newly transpiled file to repo
- Commit ONLY the `@babel/runtime` package in `package.json` file.  This is the only package required for successful build on server

## Generating fizz-chart
- From TS file, import FizzChart class directly from library file:
`import {FizzChart}  from "../../../../../assets/js/fizz-charts";`
- FizzChart requires a config to be passed in as a param, which is defined in JSON file in sub-folder of component directory, data, and a 'container' object, which has a single property with the template reference as its value. This element will be contain the generated svg element for the chart.
- The config file has label text and font-size properties defined.
- Most styles are hard-coded within `fizz-chart.js`.  Some properties can be applied with stylesheet with appropriate selector.