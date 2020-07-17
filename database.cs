using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

using static teleinfonet.Display;
using static System.Console;

namespace teleinfonet
{
    public abstract class Backend
    {
        protected DbConnection cnx;
        protected bool persistconnection;

        protected DbCommand cmdAdd;

        protected DbParameter pdateinsert;
        protected DbParameter pIDDEVICE;
        protected DbParameter pOPTARIF;
        protected DbParameter pIDXBASE;
        protected DbParameter pIDXHC;
        protected DbParameter pIDXHP;
        protected DbParameter pIDXEJPHN;
        protected DbParameter pIDXEJPHPM;
        protected DbParameter pBBRHCJB;
        protected DbParameter pBBRHPJB;
        protected DbParameter pBBRHCJW;
        protected DbParameter pBBRHPJW;
        protected DbParameter pBBRHCJR;
        protected DbParameter pBBRHPJR;
        protected DbParameter pPTEC;
        protected DbParameter pIINST;
        protected DbParameter pPAPP;
        protected DbParameter pHHPHC;

        public Backend( bool persistconnection )
        {
            this.persistconnection = persistconnection;
        }

        public abstract bool Write( List<Dictionary<string,string>> entries );
    }

    public class NoBackend : Backend
    {
        public NoBackend() : base( false ) {}
        public override bool Write( List<Dictionary<string,string>> entries ) { return true; }
    }

    public class MySqlBackend : Backend
    {
        public MySqlBackend( string cnxstr, bool persistconnection ) : base( persistconnection )
        {
            cnx = new MySqlConnection(cnxstr);

            cmdAdd = new MySqlCommand(
                "INSERT INTO teleinfo (dateinsert, IDDEVICE, OPTARIF, IDXBASE, IDXHC, IDXHP, IDXEJPHN, IDXEJPHPM, BBRHCJB, BBRHPJB, BBRHCJW, BBRHPJW, BBRHCJR, BBRHPJR, PTEC, IINST, PAPP, HHPHC)"
                + " VALUES (?dateinsert, ?IDDEVICE, ?OPTARIF, ?IDXBASE, ?IDXHC, ?IDXHP, ?IDXEJPHN, ?IDXEJPHPM, ?BBRHCJB, ?BBRHPJB, ?BBRHCJW, ?BBRHPJW, ?BBRHCJR, ?BBRHPJR, ?PTEC, ?IINST, ?PAPP, ?HHPHC)",
               (MySqlConnection)cnx);

            pdateinsert = new MySqlParameter("?dateinsert", MySqlDbType.DateTime) { IsNullable = false };
            cmdAdd.Parameters.Add(pdateinsert);
            pIDDEVICE = new MySqlParameter("?IDDEVICE", MySqlDbType.VarChar) { IsNullable = false };
            cmdAdd.Parameters.Add(pIDDEVICE);
            pOPTARIF = new MySqlParameter("?OPTARIF", MySqlDbType.VarChar) { IsNullable = false };
            cmdAdd.Parameters.Add(pOPTARIF);
            pIDXBASE = new MySqlParameter("?IDXBASE", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pIDXBASE);
            pIDXHC = new MySqlParameter("?IDXHC", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pIDXHC);
            pIDXHP = new MySqlParameter("?IDXHP", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pIDXHP);
            pIDXEJPHN = new MySqlParameter("?IDXEJPHN", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pIDXEJPHN);
            pIDXEJPHPM = new MySqlParameter("?IDXEJPHPM", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pIDXEJPHPM);
            pBBRHCJB = new MySqlParameter("?BBRHCJB", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHCJB);
            pBBRHPJB = new MySqlParameter("?BBRHPJB", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHPJB);
            pBBRHCJW = new MySqlParameter("?BBRHCJW", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHCJW);
            pBBRHPJW = new MySqlParameter("?BBRHPJW", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHPJW);
            pBBRHCJR = new MySqlParameter("?BBRHCJR", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHCJR);
            pBBRHPJR = new MySqlParameter("?BBRHPJR", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pBBRHPJR);
            pPTEC = new MySqlParameter("?PTEC", MySqlDbType.VarChar) { IsNullable = true };
            cmdAdd.Parameters.Add(pPTEC);
            pIINST = new MySqlParameter("?IINST", MySqlDbType.Byte) { IsNullable = true };
            cmdAdd.Parameters.Add(pIINST);
            pPAPP = new MySqlParameter("?PAPP", MySqlDbType.Int32) { IsNullable = true };
            cmdAdd.Parameters.Add(pPAPP);
            pHHPHC = new MySqlParameter("?HHPHC", MySqlDbType.VarChar) { IsNullable = true };
            cmdAdd.Parameters.Add(pHHPHC);

            if(persistconnection)
                cnx.Open();
        }

        public override bool Write( List<Dictionary<string,string>> entries )
        {
            bool bres = false;
/*
                CREATE TABLE teleinfo ( `dateinsert` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP , `IDDEVICE` CHAR(14) NOT NULL , `IDXBASE` INT NULL ,
 `IDXHC` INT NULL , `IDXHP` INT NULL , `IDXEJPHN` INT NULL , `IDXEJPHPM` INT NULL , 
 `BBRHCJB` INT NULL , `BBRHPJB` INT NULL , `BBRHCJW` INT NULL , `BBRHPJW` INT NULL , `BBRHCJR` INT NULL , `BBRHPJR` INT NULL , `PTEC` CHAR(4) NULL , 
 `IINST` TINYINT NULL , `PAPP` INT NULL , `HHPHC` CHAR(1) NULL , PRIMARY KEY (`dateinsert`, `IDDEVICE`)) ENGINE = MyISAM; 
*/
        DbTransaction tran = null;

            try
            {
                if( cnx.State!=ConnectionState.Open )
                    cnx.Open();

                tran = cnx.BeginTransaction();

                Verbose("Writing "+entries.Count+" entries in "+cnx.ConnectionString);
                //Verbose(ObjectDumper.Dump( entries ));
                foreach( var e in entries)
                {
                    pdateinsert.Value = Convert.ToDateTime(e["_datetime"]);
                    pIDDEVICE.Value = e["ADCO"];
                    pOPTARIF.Value = e["OPTARIF"];
                    pIDXBASE.Value = e.ContainsKey("IDXBASE") ? (int?)Convert.ToInt32(e["IDXBASE"]) : null;
                    pIDXHC.Value = e.ContainsKey("HCHC") ? (int?)Convert.ToInt32(e["HCHC"]) : null;
                    pIDXHP.Value = e.ContainsKey("HCHP") ? (int?)Convert.ToInt32(e["HCHP"]) : null;
                    pIDXEJPHN.Value = e.ContainsKey("EJP HN") ? (int?)Convert.ToInt32(e["EJP HN"]) : null;
                    pIDXEJPHPM.Value = e.ContainsKey("EJP HPM") ? (int?)Convert.ToInt32(e["EJP HPM"]) : null;
                    pBBRHCJB.Value = e.ContainsKey("BBR HC JB") ? (int?)Convert.ToInt32(e["BBR HC JB"]) : null;
                    pBBRHPJB.Value = e.ContainsKey("BBR HP JB") ? (int?)Convert.ToInt32(e["BBR HP JB"]) : null;
                    pBBRHCJW.Value = e.ContainsKey("BBR HC JW") ? (int?)Convert.ToInt32(e["BBR HC JW"]) : null;
                    pBBRHPJW.Value = e.ContainsKey("BBR HP JW") ? (int?)Convert.ToInt32(e["BBR HP JW"]) : null;
                    pBBRHCJR.Value = e.ContainsKey("BBR HC JR") ? (int?)Convert.ToInt32(e["BBR HC JR"]) : null;
                    pBBRHPJR.Value = e.ContainsKey("BBR HP JR") ? (int?)Convert.ToInt32(e["BBR HP JR"]) : null;
                    pPTEC.Value = e.ContainsKey("PTEC") ? e["PTEC"] : null;
                    pIINST.Value = e.ContainsKey("IINST") ? (int?)Convert.ToInt32(e["IINST"]) : null;
                    pPAPP.Value = e.ContainsKey("PAPP") ? (int?)Convert.ToInt32(e["PAPP"]) : null;
                    pHHPHC.Value = e.ContainsKey("HHPHC") ? e["HHPHC"] : null;

                    cmdAdd.ExecuteNonQuery();
                }
                tran.Commit();
                bres = true;

                if(!persistconnection)
                    cnx.Close();
            }
            catch (System.Exception ex)
            {
                WriteError("Got an exception running the transaction to the database : "+ex.Message );
            }
            finally
            {
                if(tran!=null && tran.Connection!=null)
                    tran.Dispose();
            }

            return bres;
        }
    }

    class SqlBuilder
    {
        public static Backend Build( string dbtype, string cnxstr, bool persistconnection )
        {
            Backend be = null;

            switch ( dbtype.ToLower() )
            {
                case "mysql":
                    be = new MySqlBackend( cnxstr, persistconnection );
                    break;
                case "none" :
                    be = new NoBackend();
                    break;
                default:
                    new Exception("Unable to select a Database backend, wrong argument.");
                    break;
            }

            return be;
        }
    }
}