<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestForm.aspx.cs" Inherits="CustomProxy.TestForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1><%= DateTime.Now %></h1>
            <h2>[<%= (Request != null && Request.LogonUserIdentity != null ? Request.LogonUserIdentity.Name : "Unknown") %>]</h2>
        </div>
    </form>
</body>
</html>
