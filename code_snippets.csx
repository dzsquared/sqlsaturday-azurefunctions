// 1 references

// external framework assemblies
#r "Newtonsoft.Json"

// namespace references
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Configuration;
using System.Xml;
using System.Data;
using System.Data.SqlClient;



// 2 writing to the log
//anything written to the log will be visible in real time as well as in the monitor section
    log.Info("Drew's example has been triggered.");


//3 document objects
internal class xNote
{
    internal string language;
    internal int id;
    internal string text;
}
internal class alldocuments
{
    internal List<xNote> documents;
}

//4 try catch
    try{
        var httpStatus = HttpStatusCode.OK;
        string responseString = "";
        dynamic data = await req.Content.ReadAsAsync<object>();



        var notesResponse = new StringContent(responseString);
        return new HttpResponseMessage() { Content = notesResponse, StatusCode = httpStatus };
    } catch (Exception ex) {
        log.Error($"C# Http trigger function exception: {ex.Message}");
        return new HttpResponseMessage() { Content = new StringContent(""), StatusCode = HttpStatusCode.InternalServerError };
    }


//5 connect to a sql server to get some notes for analysis
        alldocuments docs = new alldocuments();
        var notescollection = new List<xNote> ();
        var cnnString  = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        using(var connection = new SqlConnection(cnnString)) {

        }

//6 set a command 
string sql = @"select TOP 50 N.NID, SUBSTRING(NOTE,1,1000) AS NOTE from JOBNOTES N
                            LEFT JOIN JOBNotes_Sentiment S ON N.NId = S.NId
                            WHERE S.NId IS NULL AND N.createdby = @PID
                            ORDER BY N.NID DESC";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                var aParam = new SqlParameter("PID", SqlDbType.VarChar);
                aParam.Value = data?.pid;
                command.Parameters.Add(aParam);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                

                }
                connection.Close();
            }

//7 populate the user id, execute the statement
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // for each row in the resultset
                            // create a new xnote object
                            // set its attributes from the results
                            // add the xnote to the collection
                            xNote jobnote = new xNote();
                            jobnote.text = reader.GetString(reader.GetOrdinal("Note"));
                            jobnote.id = reader.GetInt32(reader.GetOrdinal("NID"));
                            jobnote.language = "en";
                            notescollection.Add(jobnote);
                        }
                    }
                    reader.Close();


//8 turn notescollection into json
        docs.documents = notescollection;
        string xnotejson = JsonConvert.SerializeObject(docs);

        // create an text analytics api request
        var requestData = new StringContent(xnotejson);
        using(var client = new HttpClient()) {


        }

//9 make a text analytics call
string Url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
            client.BaseAddress = new Uri(Url);
            
            // add headers to the request
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept","application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key","dac64b8bb6cb4832a692fd6abacbb648");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent body = requestData;
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // send the request and check the response
            var result = await client.PostAsync(Url, body);
            string resultContent = await result.Content.ReadAsStringAsync();
            dynamic sentiments = JsonConvert.DeserializeObject(resultContent);
            if (sentiments.documents == null)
            {
                // return a bad request code to the original request
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        error = "Bad response from Azure text analysis."
                    });
            }

//10 write results back to database
XmlDocument xmldoc = JsonConvert.DeserializeXmlNode(resultContent,"documents");
            DataSet ds = new DataSet("JsonData");
            XmlReader xr = new XmlNodeReader(xmldoc);
            DataTable dt = new DataTable();
            ds.ReadXml(xr);
            dt = ds.Tables[0];
            responseString = resultContent;

            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "JobNotes_Sentiment";
                    bulkCopy.ColumnMappings.Add("score","sentiment");
                    bulkCopy.ColumnMappings.Add("id","NId");
                    bulkCopy.WriteToServer(dt);
                    bulkCopy.Close();
                }
                connection.Close();
            }



