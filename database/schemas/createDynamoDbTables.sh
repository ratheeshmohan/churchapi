#!/bin/sh
##prerequisite
#http://docs.aws.amazon.com/cli/latest/reference/configure/
#Create a profile named admin using command " aws configure --profile admin"
#Run this command from terminal like -  bash createDynamoDbTables.sh 


#Creates Church table
aws dynamodb create-table --table-name Churches --attribute-definitions AttributeName=ChurchId,AttributeType=S --key-schema AttributeName=ChurchId,KeyType=HASH --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1 --profile admin

#Creates Family table
aws dynamodb create-table --table-name Families --attribute-definitions AttributeName=ChurchId,AttributeType=S AttributeName=FamilyId,AttributeType=S --key-schema AttributeName=ChurchId,KeyType=HASH AttributeName=FamilyId,KeyType=RANGE --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1 --profile admin

#Creates Members table
aws dynamodb create-table --table-name Members --attribute-definitions AttributeName=ChurchId,AttributeType=S AttributeName=MemberId,AttributeType=S --key-schema AttributeName=ChurchId,KeyType=HASH AttributeName=MemberId,KeyType=RANGE --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1 --profile admin
