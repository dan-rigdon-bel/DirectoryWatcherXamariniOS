#!/bin/bash
set -e

has_xcodeProj=false;
has_xcodeTools=true;
has_lipo=true;
is_ready_for_ios_build=false;
did_build_ios_successfully=false;

function xcodeProjCheck () {
	printf " Checking for xcodeproj                "
	if test ! -d DirectoryWatcher.xcodeproj;
	then
	  printf "...DirectoryWatcher.xcodeproj NOT FOUND\n"
	  has_xcodeProj=false
	fi

	if test -d DirectoryWatcher.xcodeproj;
	then
		printf "...Ok\n"
		has_xcodeProj=true
	fi
}

function xcodeToolsCheck () {
    printf " Checking for XCode command line tools "
	XCCOMMANDS="xcodebuild"
	for i in $XCCOMMANDS
	do
		command -v $i >/dev/null && continue || { printf "...$i NOT FOUND\n"; has_xcodeTools=false; }
	done

	if $has_xcodeTools
	then
		printf "...Ok\n"
		has_xcodeTools=true;
	fi
}

function lipoCheck () {
	printf " Checking for lipo                     "
	LIPOCOMMANDS="lipo"
	for i in $LIPOCOMMANDS
	do
		command -v $i >/dev/null && continue || { printf "...$i NOT FOUND.\n"; has_lipo=false;}
	done

	if $has_lipo
	then
		printf "...Ok\n"
	fi
}

function checkiOS () {
	echo ""
	echo "iOS Pre-build check---------------------------------------------"
	xcodeProjCheck
	xcodeToolsCheck
	lipoCheck

	if $has_xcodeProj && $has_xcodeTools && $has_lipo
	then
		is_ready_for_ios_build=true;
		echo "STATUS: Ready to build libDirectoryWatcher.a for iOS"
	fi

	if ! $is_ready_for_ios_build
	then
		echo "STATUS: NOT ready for iOS build.  Please address the following:"
		if ! $has_xcodeProj;
		then
			echo " ---ERROR: Script in wrong location or xcodeproj missing----------------"
			echo "  This script must be placed in DirectoryWatcherNative/DirectoryWatcher folder."
			echo "  This folder should contain the DirectoryWatcher.xcodeproj file that is used in "
			echo "  the command line build.  If this script is in the proper folder,"
			echo "  and you are getting this error message, then you are likely missing"
			echo "  the XCode project.  Go to https://github.com/dbelcher/DirectoryWatcherTouch"
			echo "  to download."
		fi

		if ! $has_xcodeTools
		then
			echo " ---ERROR: XCode Command Line Tools Missing-----------------------------"
			echo "  Building the universal binary requires xcodebuild.  This utility"
			echo "  is included with Apple's XCode Command Line Tools.  As of XCode 4.3,"
			echo "  Command Line Tools are optional.  To install Command Line tools, open"
			echo "  XCode > Preferences > Downloads Tab > Components > Command Line Tools."
			echo "  Download and install.  Be sure to close/restart your terminal session"
			echo "  before running this command again."
		fi

		if ! $has_lipo
		then
			echo " ---ERROR: Lipo missing or not in PATH----------------------------------"
			echo "  Building the universal binary requires lipo.  If you are seeing this"
			echo "  error, it is likely that lipo is not in your path.  Verify that lipo"
			echo "  is in the /usr/bin folder and that this folder is in your PATH."
		fi
	fi
}



function createBuildFoldersForIOS () {
	if test ! -d build
	then
		mkdir build
	fi

	#check to make sure the folder was created successfully
	if test ! -d build
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi

	cd build

	if test ! -d Release-ios
	then
		mkdir Release-ios
	fi

	#check to make sure the folder was created successfully
	if test ! -d Release-ios
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi

	cd ..
}

function help () {
	echo ""
    echo "build_mobile.sh - bash shell script for building DirectoryWatcher for mobile platforms."
    echo "usage: ./build_mobile.sh argument"
    echo ""
    echo "    argument:   description:"
    echo ""
	echo "    check       perform pre-requisites check for each platform"
	echo "    help        display this screen"
	echo ""
}

function iOSFinishedMessage () {
	echo "STATUS: iOS Build Complete.  libDirectoryWatcher is in build/Release-ios"
}


function buildForiOS() {
	echo "iOS Build-------------------------------------------------------"
	echo "Making static libDirectoryWatcher.a for iOS..."

	printf " Compiling x64_86 version (Simulator)  "
	# add: > /dev/null  to suppress output on xcodeBuilds
	xcodeBuild -project DirectoryWatcher.xcodeproj -target DirectoryWatcher -sdk iphonesimulator -arch x86_64 -configuration Release clean build
	printf "...Done\n";
	cd build/Release-iphonesimulator
	mv libDirectoryWatcher.a libDirectoryWatcher-x86_64.a
	mv libDirectoryWatcher-x86_64.a ../../build/Release-ios
	cd ../..


	printf " Compiling arm64 version               "
	xcodeBuild -project DirectoryWatcher.xcodeproj -target DirectoryWatcher -sdk iphoneos -arch arm64 -configuration Release clean build
	printf "...Done\n";
	cd build/Release-iphoneos
	mv libDirectoryWatcher.a libDirectoryWatcher-arm64.a
	mv libDirectoryWatcher-arm64.a ../../build/Release-ios
	cd ../..

	printf " Creating Universal Binary             "
	cd build/Release-ios
	lipo -create -output libDirectoryWatcher.a libDirectoryWatcher-x86_64.a libDirectoryWatcher-arm64.a
	printf "...Done\n";
	cd ../..

	did_build_ios_successfully=true;
}

if [ "$1" = "help" ] || [ "$1" = "?" ]
then
    help
elif [ "$1" = "check" ]
then
	checkiOS
elif [ "$1" = "" ]
then

	checkiOS
	createBuildFoldersForIOS

	if $is_ready_for_ios_build;
	then
		buildForiOS
	fi

	if $did_build_ios_successfully;
	then
		iOSFinishedMessage
	fi
else
    echo "ERROR: unknown argument.  build_mobile.sh help"
    help
fi

exit 0
