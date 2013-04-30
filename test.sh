#!/bin/bash

while :
do
    wget "http://localhost:${1}/" --output-document=/dev/null 
done



