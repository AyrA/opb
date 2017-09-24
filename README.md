# opb
Offline Pirate Bay Browser

## About

This application allows you to browse the pirate bay offline.

## Downloading OPB

Either Download from the master branch and just run it,
or go to [the releases page](https://github.com/AyrA/opb/releases) to get the latest precompiled version

## How to Download Torrents

OPB **does not** contains a torrent client.
To download torrents, you need to have a torrent client installed that registered magnet links.
Just double click on an Entry to activate the magnet link.
If you don't want to install a Torrent client, check out [QuickTorrent](https://github.com/AyrA/QuickTorrent).
Place it into the application directory and it will be launched when you double click an entry.

## Database

If you need the database, head over to https://thepiratebay.org/static/dump/csv/.
If you prefer to use the TOR network: http://uj3wazyk5u4hnvtk.onion/static/dump/csv/.

The TOR network download seems to be more stable as of now.
Using normal Connections did terminate the download almost always for me.

## CLI

This Application can be started from the terminal

    OPB.EXE /I <file> | [/H] /S <term>
    
    /I <file>    - Import the given file
    /S <term>    - Searches for the given term and outputs name and hash
    /H           - Don't output name, just hashes
