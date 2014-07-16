<%@ Page language="c#" Codebehind="Default.aspx.cs" AutoEventWireup="false" Inherits="AspNetResources.CustomErrors3._Default" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
<HEAD>
    <title>Default</title>
</HEAD>
<body >
    <form id="Form1" method="post" runat="server">
        <p>To see custom error pages
            <ol>
                <li>request a page that doesn't exist, eg: test.aspx, or</li>
                <li>uncomment the code in Page_OnLoad and let it throw an exception</li>
            </ol>
        </p>
    </form>
</body>
</HTML>
