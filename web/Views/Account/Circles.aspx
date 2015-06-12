<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Yavsc.Model.Circles.CircleInfoCollection>" %>
<asp:Content ID="initContent" ContentPlaceHolderID="init" runat="server">
</asp:Content>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Model==null) { %>
No circle yet
<% } else { %>

<% foreach (CircleInfo ci in Model) { %>
 <%= ci.Title %>
 <%= ci.Id %>
 <br/>
<% }} %>
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">
</asp:Content>
