<%@ Page Title="Json reader error" Language="C#" Inherits="System.Web.Mvc.ViewPage<JsonReaderError>" MasterPageFile="~/Models/App.master" %>
 <asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

 <h2><%= Html.Encode(Model.Excepx.Message)%></h2>

 <pre>
 <code>
 <%= Html.Encode(Model.Text) %>
 </code></pre>

 </asp:Content>

