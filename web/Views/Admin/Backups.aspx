<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<%=Html.ActionLink("Create a database backup", "CreateBackup")%><br/>
<%=Html.ActionLink("Restaurations", "Restore")%><br/>
</asp:Content>
