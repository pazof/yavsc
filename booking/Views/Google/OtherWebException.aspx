<%@ Page Title="Google error message" Language="C#" Inherits="System.Web.Mvc.ViewPage<OtherWebException>" MasterPageFile="~/Models/App.master" %>
 <asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
 <h2><%= Html.Encode(Model.Title)%></h2>
 <pre>
 <code>
 <%= Html.Encode(Model.Content) %>
 </code></pre>
 </asp:Content>

