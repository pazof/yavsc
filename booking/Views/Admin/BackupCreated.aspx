<%@ Page Title="Backup created" Language="C#" MasterPageFile="~/Models/AppAdmin.master" Inherits="System.Web.Mvc.ViewPage<Export>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<div><h2>Error message </h2> <%= Html.Encode(Model.Error) %></div>
<div><h2>Message </h2> <%= Html.Encode(Model.Message) %></div>
<div><h2>File name</h2> <%= Html.Encode(Model.FileName) %></div>
<div><h2>Exit Code</h2> <%= Html.Encode(Model.ExitCode) %></div>
</asp:Content>
