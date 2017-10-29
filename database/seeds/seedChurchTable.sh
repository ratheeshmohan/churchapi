#!/bin/sh
##prerequisite
#http://docs.aws.amazon.com/cli/latest/reference/configure/
#Create a profile named admin using command " aws configure --profile admin"

#Run this command from terminal like -  bash seedChurchTable.sh 

aws dynamodb batch-write-item --request-items file://church.json --profile admin