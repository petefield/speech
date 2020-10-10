$FetchTokenHeader = @{
    'Content-type'              = 'application/x-www-form-urlencoded'
    'Content-Length'            = '0'
    'Ocp-Apim-Subscription-Key' = '9bed6c994a21401b8ef9e4dc086d3aac'
}
$OAuthToken = Invoke-RestMethod -Method POST -Uri https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken -Headers $FetchTokenHeader
$OAuthToken