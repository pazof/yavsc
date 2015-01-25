<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<TaskOutput>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<h1><%=Html.Encode(ViewData["BackupName"])%> Restauration</h1>
<div><h2>Error message </h2> <%= Html.Encode(Model.Error) %></div>
<div><h2>Message </h2> <%= Html.Encode(Model.Message) %></div>
<div><h2>Exit Code</h2> <%= Html.Encode(Model.ExitCode) %></div>

</asp:Content>
