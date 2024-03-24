# LiveOps

An attempt to update Unity remotely allowing content to be updated without requiring an update to original version.  There is a central configuration file stored in Blob Storage with an example WAV file.  The Unity program will load and collect this configuration file.  The test app will then takes an email address to mimic some form or config logging.  On checking for updates Unity will download the file, update the necessary components and then update the Azure Table Storage.  The final step is to allow for a rough view on what versions and files the userbase are using.

The logic resides in [CheckForUpdatesService.cs](https://pages.github.com/).

The code can't be ran locally as I've removed the hardcoded settings, I wasn't able to use KeyVault as part of the Azure free $200.  There is a video that demonstrates the POC.  The first 'Play Sound' uses the first sound file, after 'Check for updates' is pressed a new sound file will be played.
