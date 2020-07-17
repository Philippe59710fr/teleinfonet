#!/bin/bash

export DOTNET_BUNDLE_EXTRACT_BASE_DIR=~/.net
./teleinfonet once=false output=inline wfrequency=10 afrequency=60 bufferfile=~/teleinfo_buffer.json persistconnection=false verbose=false cnxstr=Database%3Dteleinfo\;Data%20Source%3D192.168.1.3\;Port%3D3307\;User%20Id%3Dmyuser\;Password%3Dmypassword >>~/teleinfo.out 2>>~/teleinfo.err
