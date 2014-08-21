# Overview
This utility syncronizes a set of mappings from source to destination folders. It can optionally keep backups of old versions so no data is ever lost. There are a number of options for customizing the synchronization.

# Features
* A set of automated tests will run in debug builds.
* Can detect moved files to reduce copy time
* When modifying or deleting, the old destination file is sent to the recycle bin
* If there will not be enough space to synchronise a mapping it will be skipped and an alert is displayed.
* Progress is displayed based on the amount of data left to be copied.
* Persistent configuration for a list of mappings to synchronize and options. 
* A dry run can be performed to see what would be copied and deleted.
* Can ignore files like desktop.ini

# Mapping Options
* Enabled turns the mapping on, otherwise it is ignored.
* Ignore Timestamp means that only name and size are considered when deciding whether to update or ignore a file that exists in both source and destination folders.
* Never Delete means that destination files are never deleted even if they no longer exist in the source tree.
* Never Backup means never send to the recycle bin

# TODO
* Config file versioning.
* Adding ignore files to each mapping configuration.
* Settings for maximum space usage and heuristic for automatically cleaning up recycle bin.





