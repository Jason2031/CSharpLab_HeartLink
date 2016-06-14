string table,password;
SqlCommand.Parameters.Add()
String sql="select * from @table where password=@password"
cmd= new SqlCommand(sql,sqlConn);
SqlParameter param=new SqlParameter("@table",table);
cdm.Parameters.Add("@table",OldDbType.VarChar,20).value=table;
cmd.ExecuteNonQuery();


SqlCommand sqlcom =new SqlCommand( "update homework set tid=@tid,title=@title,content=@content where hid=@hid");
// string sqlcomd = "update homework set tid='" + tid + "',title='" + title + "',content='" + content +"' where hid='" + hid + "'";
SqlCommand sql = new SqlCommand("select ID from @table where password=@password");
sql.Parameters.Add(new SqlParameter("@table",table));
sql.Parameters.Add(new SqlParameter("@password",password));
 sqlcom.Parameters.Add(new SqlParameter("@tid", tid));
 sqlcom.Parameters.Add(new SqlParameter("@hid", hid));
 sqlcom.Parameters.Add(new SqlParameter("@title", title));
 sqlcom.Parameters.Add(new SqlParameter("@file", file));

 SqlConnection conn = getConnection();
        command.Connection = conn;
        try
        {
            conn.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            conn.Close();
            command.Dispose();
        }
    }