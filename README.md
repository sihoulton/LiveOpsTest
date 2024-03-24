# LiveOps

An attempt to update Unity remotely allowing content to be changed without requiring a new version. 

There is a central configuration file stored in blob storage with an example WAV file. The Unity program will load and collect this configuration file. The test app then takes an email address to mimic some form of user identification and sign-on. On checking for updates Unity will download the file, update the necessary components, and then update the Azure Table Storage. The final step is to generate a rough view of what versions of files the user base has.

The logic resides in [CheckForUpdatesService.cs](https://github.com/sihoulton/LiveOpsTest/blob/master/Assets/CheckForUpdatesService.cs).

The code can't be run locally as I've removed the hardcoded settings which exist because I wasn't able to use KeyVault as part of the Azure free credits. 

There is a video that demonstrates the POC.  The first 'Play Sound' uses the original sound file, after 'Check for updates' is pressed a new sound file will be downloaded and played. The [video](https://github.com/sihoulton/LiveOpsTest/blob/master/Demo.mp4) can be downloaded by clicking View Raw.

