# How to host several instances of the same WebAPI in IIS Express on Windows 8.1
## General notes
* I'm not sure if the administrator part is necessary in this situation.
* I know that the administrator part is necessary if you want to let external PC's invoke the instances you host.
* Sometimes multiple IIS Express icons show up in the Notification Area.
* Visual Studio might create new site-entries when you have changed the configuration file manually.
    * If there are name clashes Visual Studio will name the new entries `<project name> (<number>)` i.e `Event (1)`

## Prerequisites
* Compile your program to have an updated .dll file to host from (you don't need to know the location for now).
* Run the WebAPI at least once from Visual Studio.
* Make sure that IIS Express is not running (check the white/blue IIS Express icon in the Notification Area).

## Step 1 - Configuration
* Open the file `applicationhost.config` in `C:\Users\<your user name>\Documents\IISExpress\config` in a text editor.
    * This is an XML-file with the configuration for IIS Express.
* Locate the `<sites>`-tag.
* Find the current entry of the given WebAPI you want to multihost. I.e. `<site name="Event"...`
* Copy the entry to right before the `<siteDefaults>`-tag.
* Change the name of the new entry to something like `name="Event1"`
* Give the entry a new id (just take the next integer in the list).
* Change the port of the instance:
    * Locate the `<bindings>`-tag inside your `<site>` and `</site>` tags.
    * If there is more than one entry of `<binding>`-tags you will have to update all ports to one that is unused on your machine.
    * I could just increment the number by 1, but make sure the port is not already in use by one of the other `<site>`-entries.
* Remember to save the edited configuration file.

## Step 2 - Locating the IIS Express server-program.
* Run a Command Prompt as Administrator:
    * Search for `cmd` in the Start Menu.
    * Right click the `Command Prompt`/`Kommandoprompt` entry and select `Run as administrator`/`Kør som administrator`
    * Grant access.
* Navigate to your `IIS Express`-installation directory.
    * The next points assumes that this directory is located at `C:\Program Files\IIS Express\`
        * `cd /` to get to the root of your system drive.
        * `cd "Program Files\IIS Express"`
* Create new Command Prompt corresponding to the number of WebAPIs that should be hosted
    * Use `start cmd` to create new Command Prompt-windows.

### Step 3 - Running the WebAPI
* To start a WebAPI write the following command:
    * `iisexpress /site:<site name>` where `<site name>` is the name it was given in the configuration file during step 1.
    * Alternatively:
        * `iisexpress /siteid:<site id>` where `<site id>` is the id of the site in the configuration file.
    * If you accidentally runs the server with wrong parameters look at step 4.

### Step 4 - Stopping the sites
* When you have finished testing your WebAPI (or ran it with wrong parameters) you can stop the sites by doing the following.
    * Locate the IIS Express icon in the Notification Area.
    * Right click it, and choose the site you want to stop
    * Press `Stop site` in the submenu of the site you want to stop.
