<%@ Page Language="C#" MasterPageFile="~/Models/AppAdmin.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<%=Html.TranslatedActionLink("Create a database backup", "CreateBackup")%><br/>
<%=Html.TranslatedActionLink("Restaurations", "Restore")%><br/>
</asp:Content>
