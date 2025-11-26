#/bin/sh

# Copyright (c) 2021, Mapache Digital
# Version: 1.0
# Author: Samuel Kobelkowsky
# Email: samuel@mapachedigital.com
#
# update the database of a .NET proyect using SQLite

PATH=/bin:/usr/bin:/c/Program\ Files/dotnet:/opt/homebrew/bin:/usr/local/share/dotnet

# Configure the following variables:
SOLUTION="../BlogAdecco.sln"
SQLPROJECT="../SqliteMigrations"
DATABASEPROVIDER="Sqlite"

usage() { echo -e "\nUsage $0 migration-name."; }

while getopts ":h:d" opt; do
	case $opt in
		h) usage; exit 0;;
		m) MIGRATIONNAME=$OPTARG;;
		\?) usage; echo -e "\nError: invalid option: ${OPTARG}." >&2; exit 1;;
	esac	
done

shift $((OPTIND-1))
MIGRATIONNAME="$1"

dotnet ef database update $MIGRATIONNAME --project $SQLPROJECT -- --DatabaseProvider $DATABASEPROVIDER
dotnet build $SOLUTION