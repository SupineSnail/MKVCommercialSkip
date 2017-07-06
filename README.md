## MKV Commercial Skip

This is a little tool I created to help with my live recordings on EMBY. As such, it is a little raw right now.
Automated MKV tool that works with Comskip to add in chapters in your recordings so you can skip commercials. 

Takes in an MKV file, runs Comskip and adds chapters to that MKV file (overwriting original)

### Requirements

I do not include Comskip with this, as you need the Donator version in order to run Comskip on MKV files. You can donate to Comskip and get your own version here: http://www.kaashoek.com/comskip/
Once you have Comskip, copy all the files (you can use the included Comskip.ini for my settings, or overwrite for defaults) to Dependencies/Comskip

### Usage

MKVCommercialSkip -file {fileName} [-log]
log is optional. If not entered, output will be to console.

### Notes

I use this in combination with Emby. In server settings -> Live TV -> Settings -> Recording Post Processing I point to the EXE and use the following for arguments: '-file "{path}" -log'