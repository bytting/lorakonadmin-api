//  lorakonadmin-api - Web API to manage a Lorakon sepctrum database
//  Copyright (C) 2017  Norwegian Radiation Protection Autority
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Authors: Dag Robole,

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web.Http;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace lorakonadmin_api.Controllers
{    
    [BasicAuthentication]
    public class SpectrumController : ApiController
    {
        ConnectionStringSettings LorakonUsersConnectionStrings = ConfigurationManager.ConnectionStrings["nrpa_lorakon_users"];
        ConnectionStringSettings LorakonConnectionStrings = ConfigurationManager.ConnectionStrings["nrpa_lorakon"];

        public SpectrumController()
        {
            if (LorakonUsersConnectionStrings == null || String.IsNullOrEmpty(LorakonUsersConnectionStrings.ConnectionString))
                throw new Exception("Fatal error: missing lorakon users connecting string in web.config file");

            if (LorakonConnectionStrings == null || String.IsNullOrEmpty(LorakonConnectionStrings.ConnectionString))
                throw new Exception("Fatal error: missing lorakon connecting string in web.config file");
        }

        [HttpGet]
        public IEnumerable<Models.AccountBasic> get_all_accounts_basic()
        {
            List<Models.AccountBasic> accountList = new List<Models.AccountBasic>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select ID, vchName from Account order by vchName", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.AccountBasic user = new Models.AccountBasic(
                            (Guid)reader["ID"],
                            reader["vchName"].ToString());
                        accountList.Add(user);
                    }
                }
            }

            return accountList;
        }

        [HttpGet]
        public IEnumerable<Models.LogEntry> get_log_entries(string from, string to)
        {            
            DateTime dtFrom = DateTime.ParseExact(from, "yyyyMMdd_hhmmss", null);
            DateTime dtTo = DateTime.ParseExact(to, "yyyyMMdd_hhmmss", null);

            List<Models.LogEntry> logList = new List<Models.LogEntry>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();

                SqlCommand command = new SqlCommand("proc_spectrum_log_select", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FromDate", dtFrom);
                command.Parameters.AddWithValue("@ToDate", dtTo);
                
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.LogEntry ent = new Models.LogEntry(
                            Convert.ToDateTime(reader["CreateDate"]),
                            Convert.ToInt32(reader["Severity"]),
                            reader["Message"].ToString());
                        logList.Add(ent);
                    }
                }
            }

            return logList;
        }

        [HttpGet]
        public IEnumerable<Models.LogEntry> get_log_entries_severity(string from, string to, int severity)
        {
            DateTime dtFrom = DateTime.ParseExact(from, "yyyyMMdd_hhmmss", null);
            DateTime dtTo = DateTime.ParseExact(to, "yyyyMMdd_hhmmss", null);

            List<Models.LogEntry> logList = new List<Models.LogEntry>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();

                SqlCommand command = new SqlCommand("proc_spectrum_log_select_severity", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FromDate", dtFrom);
                command.Parameters.AddWithValue("@ToDate", dtTo);
                command.Parameters.AddWithValue("@Severity", severity);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.LogEntry ent = new Models.LogEntry(
                            Convert.ToDateTime(reader["CreateDate"]),
                            Convert.ToInt32(reader["Severity"]),
                            reader["Message"].ToString());
                        logList.Add(ent);
                    }
                }
            }

            return logList;
        }

        [HttpGet]
        public IEnumerable<Models.SpectrumInfoBasic> get_spectrum_info_latest(string from, string to, string accid, string samp, bool appr, bool rej)
        {
            // /api/spectrum/get_spectrum_info_latest?from=20100101_120000&to=20171201_120000&accid=&samp=&appr=true&rej=false

            DateTime dtFrom = DateTime.ParseExact(from, "yyyyMMdd_hhmmss", null);
            DateTime dtTo = DateTime.ParseExact(to, "yyyyMMdd_hhmmss", null);

            List<Models.SpectrumInfoBasic> specList = new List<Models.SpectrumInfoBasic>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();

                SqlCommand command = new SqlCommand("", conn);
                command.CommandText = @"
select a.vchName as 'Laboratory', 
	sil.Operator as 'Operator',
    sil.ExternalID as 'ExternalID',
	cast(sil.CreateDate as datetime2(0)) as 'CreateDate',
	cast(sil.AcquisitionDate as datetime2(0)) as 'AcquisitionDate',
	cast(sil.ReferenceDate as datetime2(0)) as 'ReferenceDate',
	sil.SampleType as 'SampleType', 
	sil.SampleComponent as 'SampleComponent',
	sil.Approved as 'Approved',	
    sil.ApprovedStatus as 'ApprovedStatus',
    sil.Rejected as 'Rejected',
    sil.ID as 'ID'
from SpectrumInfoLatest sil 
inner join Account a on a.ID = sil.AccountID
where 1=1
";

                if (!String.IsNullOrEmpty(accid))
                {                    
                    command.CommandText += " and AccountID like '" + accid + "'";
                }

                if (!String.IsNullOrEmpty(samp))
                {
                    command.CommandText += " and SampleType like '%" + samp + "%'";
                }

                command.CommandText += " and Approved = @Approved";
                command.Parameters.AddWithValue("@Approved", appr ? "1" : "0");

                command.CommandText += " and Rejected = @Rejected";
                command.Parameters.AddWithValue("@Rejected", rej ? "1" : "0");

                command.CommandText += " and AcquisitionDate between @dateFrom and @dateTo";
                command.Parameters.AddWithValue("@dateFrom", dtFrom);
                command.Parameters.AddWithValue("@dateTo", dtTo);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.SpectrumInfoBasic spec = new Models.SpectrumInfoBasic();
                        spec.ID = new Guid(reader["ID"].ToString());
                        spec.AccountName = reader["Laboratory"].ToString();
                        spec.Operator = reader["Operator"].ToString();
                        spec.ExternalID = reader["ExternalID"].ToString();
                        spec.CreateDate = Convert.ToDateTime(reader["CreateDate"]);
                        spec.AcquisitionDate = Convert.ToDateTime(reader["AcquisitionDate"]);
                        spec.ReferenceDate = Convert.ToDateTime(reader["ReferenceDate"]);
                        spec.SampleType = reader["SampleType"].ToString();
                        spec.SampleComponent = reader["SampleComponent"].ToString();
                        spec.Approved = Convert.ToBoolean(reader["Approved"]);
                        spec.ApprovedStatus = reader["ApprovedStatus"].ToString();
                        spec.Rejected = Convert.ToBoolean(reader["Rejected"]);
                        specList.Add(spec);
                    }
                }
            }

            return specList;
        }        

        [HttpGet]
        public Models.AccountName get_account_name(string id)
        {
            // /api/spectrum/get_account_name/D31B6A79-570C-4B73-9A4F-840751CC8CF4

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select vchName from Account where ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);

                object nm = cmd.ExecuteScalar();
                if(nm == null || nm == DBNull.Value)                
                    return new Models.AccountName();
                else                
                    return new Models.AccountName(nm.ToString());
            }            
        }        

        [HttpGet]
        public IEnumerable<Models.ValidationRule> get_all_validation_rules()
        {
            // /api/spectrum/get_all_validation_rules

            List<Models.ValidationRule> rulesList = new List<Models.ValidationRule>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from SpectrumValidationRules", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.ValidationRule rule = new Models.ValidationRule(
                            (Guid)reader["ID"],
                            reader["NuclideName"].ToString(),
                            Convert.ToSingle(reader["ActivityMin"]),
                            Convert.ToSingle(reader["ActivityMax"]),
                            Convert.ToSingle(reader["ConfidenceMin"]),
                            Convert.ToBoolean(reader["CanBeAutoApproved"]));
                        rulesList.Add(rule);
                    }
                }
            }

            return rulesList;
        }

        [HttpPost]
        public IHttpActionResult insert_validation_rule([FromBody]Models.ValidationRule rule)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("insert into SpectrumValidationRules values (@ID, @NuclideName, @ActivityMin, @ActivityMax, @ConfidenceMin, @CanBeAutoApproved)", conn);
                cmd.Parameters.AddWithValue("@ID", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@NuclideName", rule.NuclideName);
                cmd.Parameters.AddWithValue("@ActivityMin", rule.ActivityMin);
                cmd.Parameters.AddWithValue("@ActivityMax", rule.ActivityMax);
                cmd.Parameters.AddWithValue("@ConfidenceMin", rule.ConfidenceMin);
                cmd.Parameters.AddWithValue("@CanBeAutoApproved", rule.CanBeAutoApproved);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult update_validation_rule(string id, [FromBody]Models.ValidationRule rule)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumValidationRules set ActivityMin=@ActivityMin, ActivityMax=@ActivityMax, ConfidenceMin=@ConfidenceMin, CanBeAutoApproved=@CanBeAutoApproved where NuclideName=@NuclideName", conn);
                cmd.Parameters.AddWithValue("@NuclideName", rule.NuclideName);
                cmd.Parameters.AddWithValue("@ActivityMin", rule.ActivityMin);
                cmd.Parameters.AddWithValue("@ActivityMax", rule.ActivityMax);
                cmd.Parameters.AddWithValue("@ConfidenceMin", rule.ConfidenceMin);
                cmd.Parameters.AddWithValue("@CanBeAutoApproved", rule.CanBeAutoApproved);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult delete_validation_rule(string name)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("delete from SpectrumValidationRules where NuclideName=@NuclideName", conn);
                cmd.Parameters.AddWithValue("@NuclideName", name);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IEnumerable<Models.GeometryRule> get_all_geometry_rules()
        {
            List<Models.GeometryRule> rulesList = new List<Models.GeometryRule>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from SpectrumGeometryRules", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.GeometryRule rule = new Models.GeometryRule(
                            (Guid)reader["ID"],
                            reader["Geometry"].ToString(),
                            reader["Unit"].ToString(),
                            Convert.ToSingle(reader["Minimum"]),
                            Convert.ToSingle(reader["Maximum"]));
                        rulesList.Add(rule);
                    }
                }
            }

            return rulesList;
        }

        [HttpGet]
        public Models.GeometryRule get_geometry_rule(string id)
        {
            // /api/spectrum/get_geometry_rule/e0eb4bd0-586c-4efe-bf2d-d5d34b436f8f

            Guid Id = new Guid(id);
            Models.GeometryRule rule = new Models.GeometryRule();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from SpectrumGeometryRules where ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", Id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        rule.Id = (Guid)reader["ID"];
                        rule.Geometry = reader["Geometry"].ToString();
                        rule.Unit = reader["Unit"].ToString();
                        rule.Minimum = Convert.ToSingle(reader["Minimum"]);
                        rule.Maximum = Convert.ToSingle(reader["Maximum"]);
                    }
                }
            }

            return rule;
        }

        [HttpPost]
        public IHttpActionResult insert_geometry_rule([FromBody]Models.GeometryRule rule)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("insert into SpectrumGeometryRules values (@ID, @Geometry, @Unit, @Minimum, @Maximum)", conn);
                cmd.Parameters.AddWithValue("@ID", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@Geometry", rule.Geometry);
                cmd.Parameters.AddWithValue("@Unit", rule.Unit);
                cmd.Parameters.AddWithValue("@Minimum", rule.Minimum);
                cmd.Parameters.AddWithValue("@Maximum", rule.Maximum);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult update_geometry_rule(string id, [FromBody]Models.GeometryRule rule)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumGeometryRules set Unit=@Unit, Minimum=@Minimum, Maximum=@Maximum where Geometry=@Geometry", conn);
                cmd.Parameters.AddWithValue("@Unit", rule.Unit);
                cmd.Parameters.AddWithValue("@Minimum", rule.Minimum);
                cmd.Parameters.AddWithValue("@Maximum", rule.Maximum);
                cmd.Parameters.AddWithValue("@Geometry", rule.Geometry);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult delete_geometry_rule(string name)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("delete from SpectrumGeometryRules where Geometry=@Geometry", conn);
                cmd.Parameters.AddWithValue("@Geometry", name);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }        

        [HttpGet]
        public Models.SpectrumInfo get_spectrum_info(string id)
        {
            // /api/spectrum/get_spectrum_info/431b73af-1531-4bc2-87d1-b8ccbb65d932

            Guid Id = new Guid(id);
            Models.SpectrumInfo info = new Models.SpectrumInfo();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select a.vchName, si.* from Account a, SpectrumInfo si where si.id = @SID and a.ID = si.AccountID", conn);
                cmd.Parameters.AddWithValue("@SID", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        info.ID = new Guid(id);
                        info.AccountID = (Guid)reader["AccountID"];
                        info.AccountName = reader["vchName"].ToString();
                        info.CreateDate = Convert.ToDateTime(reader["CreateDate"]);
                        info.UpdateDate = Convert.ToDateTime(reader["UpdateDate"]);
                        info.AcquisitionDate = Convert.ToDateTime(reader["AcquisitionDate"]);
                        info.ReferenceDate = Convert.ToDateTime(reader["ReferenceDate"]);
                        info.Filename = reader["Filename"].ToString();
                        info.BackgroundFile = reader["BackgroundFile"].ToString();
                        info.LibraryFile = reader["LibraryFile"].ToString();
                        info.Sigma = Convert.ToSingle(reader["Sigma"]);
                        info.SampleType = reader["SampleType"].ToString();
                        info.SampleComponent = reader["SampleComponent"].ToString();
                        info.Livetime = Convert.ToInt32(reader["Livetime"]);
                        info.Laboratory = reader["Laboratory"].ToString();
                        info.Operator = reader["Operator"].ToString();
                        info.Latitude = Convert.ToSingle(reader["Latitude"]);
                        info.Longitude = Convert.ToSingle(reader["Longitude"]);
                        info.Altitude = Convert.ToSingle(reader["Altitude"]);
                        info.LocationType = reader["LocationType"].ToString();
                        info.Location = reader["Location"].ToString();
                        info.Community = reader["Community"].ToString();
                        info.SampleWeight = Convert.ToSingle(reader["SampleWeight"]);
                        info.SampleWeightUnit = reader["SampleWeightUnit"].ToString();
                        info.SampleGeometry = reader["SampleGeometry"].ToString();
                        info.ExternalID = reader["ExternalID"].ToString();
                        info.Approved = Convert.ToBoolean(reader["Approved"]);
                        info.ApprovedStatus = reader["ApprovedStatus"].ToString();
                        info.Rejected = Convert.ToBoolean(reader["Rejected"]);
                        info.Comment = reader["Comment"].ToString();
                    }
                }
            }

            return info;
        }

        [HttpGet]
        public IHttpActionResult delete_spectrum_info(string id)
        {            
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("delete from SpectrumInfo where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult update_spectrum_info_approved(string id, bool approved)
        {
            // /api/spectrum/update_spectrum_info_approved?id=431b73af-1531-4bc2-87d1-b8ccbb65d932&approved=true

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumInfo set Approved=@Approved where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@Approved", approved);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult update_spectrum_info_rejected([FromUri]string id, [FromUri]bool rejected)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumInfo set Rejected=@Rejected where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@Rejected", rejected);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public Models.SpectrumResult get_spectrum_result(string id)
        {
            // /api/spectrum/get_spectrum_result/18218763-163E-4F46-8564-1BEE4118BECC

            Guid Id = new Guid(id);
            Models.SpectrumResult res = new Models.SpectrumResult();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from SpectrumResult where ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", Id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        res.ID = Id;
                        res.SpectrumInfoID = new Guid(reader["SpectrumInfoID"].ToString());
                        res.CreateDate = Convert.ToDateTime(reader["CreateDate"]);
                        res.UpdateDate = Convert.ToDateTime(reader["UpdateDate"]);
                        res.NuclideName = reader["NuclideName"].ToString();
                        res.Confidence = Convert.ToSingle(reader["Confidence"]);
                        res.Activity = Convert.ToSingle(reader["Activity"]);
                        res.ActivityUncertainty = Convert.ToSingle(reader["ActivityUncertainty"]);
                        res.MDA = Convert.ToSingle(reader["MDA"]);
                        res.Evaluated = Convert.ToBoolean(reader["Evaluated"]);
                        res.Approved = Convert.ToBoolean(reader["Approved"]);
                        res.ApprovedIsMDA = Convert.ToBoolean(reader["ApprovedIsMDA"]);
                        res.ApprovedStatus = reader["ApprovedStatus"].ToString();
                        res.Rejected = Convert.ToBoolean(reader["Rejected"]);
                        res.Comment = reader["Comment"].ToString();
                    }
                }
            }

            return res;
        }

        [HttpGet]
        public IEnumerable<Models.SpectrumResult> get_spectrum_results_from_specid(string specid)
        {
            // /api/spectrum/get_spectrum_results_from_specid?specid=7D12877D-AD49-4D20-819B-528BED809265

            Guid Id = new Guid(specid);
            List<Models.SpectrumResult> res = new List<Models.SpectrumResult>();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from SpectrumResult where SpectrumInfoID = @SID", conn);
                cmd.Parameters.AddWithValue("@SID", Id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.SpectrumResult r = new Models.SpectrumResult();
                        r.ID = new Guid(reader["ID"].ToString());
                        r.SpectrumInfoID = Id;
                        r.CreateDate = Convert.ToDateTime(reader["CreateDate"]);
                        r.UpdateDate = Convert.ToDateTime(reader["UpdateDate"]);
                        r.NuclideName = reader["NuclideName"].ToString();
                        r.Confidence = Convert.ToSingle(reader["Confidence"]);
                        r.Activity = Convert.ToSingle(reader["Activity"]);
                        r.ActivityUncertainty = Convert.ToSingle(reader["ActivityUncertainty"]);
                        r.MDA = Convert.ToSingle(reader["MDA"]);
                        r.Evaluated = Convert.ToBoolean(reader["Evaluated"]);
                        r.Approved = Convert.ToBoolean(reader["Approved"]);
                        r.ApprovedIsMDA = Convert.ToBoolean(reader["ApprovedIsMDA"]);
                        r.ApprovedStatus = reader["ApprovedStatus"].ToString();
                        r.Rejected = Convert.ToBoolean(reader["Rejected"]);
                        r.Comment = reader["Comment"].ToString();
                        res.Add(r);
                    }
                }
            }

            return res;
        }

        [HttpGet]
        public IHttpActionResult update_spectrum_result_approved(string id, bool approved)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumResult set Approved=@Approved where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@Approved", approved);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult update_spectrum_result_rejected(string id, bool rejected)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumResult set Rejected=@Rejected where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@Rejected", rejected);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult update_spectrum_result_evaluated(string id, bool evaluated)
        {
            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("update SpectrumResult set Evaluated=@Evaluated where ID=@ID", conn);
                cmd.Parameters.AddWithValue("@Evaluated", evaluated);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        public Models.SpectrumFileContent get_spectrum_file_content_from_specinfo(string id)
        {
            // /api/spectrum/get_spectrum_file_content_from_specinfo/F17497C9-027D-474F-A99A-71F7F3AADF2B

            Guid Id = new Guid(id);
            Models.SpectrumFileContent sfc = new Models.SpectrumFileContent();

            using (SqlConnection conn = new SqlConnection(LorakonConnectionStrings.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select SpectrumFileContent from SpectrumFile where SpectrumInfoID = @SID", conn);
                cmd.Parameters.AddWithValue("@SID", Id);

                object data = cmd.ExecuteScalar();
                if(data != null && data != DBNull.Value)
                {
                    sfc.Base64Data = Convert.ToBase64String((byte[])data);
                }
            }

            return sfc;
        }        
    }    
}
