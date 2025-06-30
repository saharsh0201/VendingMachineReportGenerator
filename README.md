This web app uses Angular CLI version 20.0.3 as the frontend and .net 9.0.300 for the backend.

#Logic
The app takes files as inputs and stores their paths. It uses CsvHelper to read and process the csv files.
It removes all the "PAYMENT_FAILED" rows from the EmpList.
Matching-
It matches the following columns (Emp_List - TList) price(extracted from product name) - used_amount,
machine_number (from machine_name) - MachineId(cleaned POS id) and created_at - created_At.

During matchine it sums up the required data for the Total Summary and also writes the rows into an
output file.

TotalSummary is stored to the database with  Id YYYYMMDD_YYYYMMDD first being the stard date and the
second is the end date.

The output file is stored in the uploads folder.

Machine Wise Data is also calculated but not stored.

The data is then displayed in the result page.

The history page shows the total summary from the database.

#Account_Creation
To create a new account run the following Query in the database
INSERT INTO Users (Username, Password) VALUES ("new username", "new password");
