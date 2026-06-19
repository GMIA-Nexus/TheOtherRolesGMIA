#!/bin/sh
tag="v${VERSION}"
repository_path="haoming37/TheOtherRoles-GM-Haoming"

response=$(curl -X POST -H "Accept: application/vnd.github.v3+json" \
-H "Authorization: token ${GitHub_TOKEN}" https://api.github.com/repos/$repository_path/releases \
-d "{\"tag_name\":\"$tag\"}")
echo $response

release_id=$(echo $response | jq '.id')

file_path="./TheOtherRolesGM.dll"
response=$(curl -X POST -H "Content-Type: $(file -b --mime-type $file_path)" \
-H "Accept: application/vnd.github.v3+json" \
-H "Authorization: token ${GitHub_TOKEN}" \
--data-binary @$file_path \
"https://uploads.github.com/repos/$repository_path/releases/$release_id/assets?name=$(basename $file_path)")
echo $response

file_path="./TheOtherRoles-GM-Haoming.v${VERSION}.zip"
response=$(curl -X POST -H "Content-Type: $(file -b --mime-type $file_path)" \
-H "Accept: application/vnd.github.v3+json" \
-H "Authorization: token ${GitHub_TOKEN}" \
--data-binary @$file_path \
"https://uploads.github.com/repos/$repository_path/releases/$release_id/assets?name=$(basename $file_path)")
echo $response