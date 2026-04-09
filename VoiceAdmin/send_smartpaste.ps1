try {
	$payload = @{ formFields = @(@{ identifier = 'name'; description = 'Name'; type = 'string' }); clipboardContents = 'Name Donald Duck, Email Donald@Disney.com, Phone 333659' }
	$dataJson = ($payload | ConvertTo-Json -Compress)
	$body = @{ dataJson = $dataJson }
	$response = Invoke-WebRequest -Uri 'http://localhost:5008/_smartcomponents/smartpaste' -Method Post -ContentType 'application/x-www-form-urlencoded' -Body $body -Verbose -UseBasicParsing
	Write-Host "Status: $($response.StatusCode)"
	Write-Host $response.Content
}
catch {
	Write-Host "Request failed:`n$($_.Exception.Message)"
	if ($_.Exception.Response) {
		$reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
		Write-Host "Response body:`n$($reader.ReadToEnd())"
	}
}
