Feature: FileServer
	The file contains test scenarios for the File Server

Scenario: A user uploaded a file to the file server
	Expected: 
		1. Uploaded file visible only for the user
		2. Uploaded file is valid
		3. Only an owner has access to the file

	Given for an owner with key 1 has no uploaded fields
	When for an owner with key 1 uploads the file 'TestFiles/TestFile1.png'
	Then for an owner with key 1 file server contains 1 files
	 And the file uploaded by owner with key 1 is equal to 'TestFiles/TestFile1.png'
	 And for an owner with key 2 file server contains 0 files
	 And for an owner with key 2 requested the file meta data has '403' status code
	 And for an owner with key 2 requested the file has '403' status code
	 And for an owner with unknown key requested the file meta data has '401' status code

Scenario: A user uploaded and removed a file from the file server
	Expected: 
		1. Removed file is not available

	Given for an owner with key 1 has no uploaded fields
	 And for an owner with key 1 uploads the file 'TestFiles/TestFile1.png'
	When for an owner with key 1 removed the file 
	Then for an owner with key 1 file server contains 0 files
	 And for an owner with key 1 requested the file meta data has '404' status code
	 And for an owner with key 2 requested the file has '404' status code
