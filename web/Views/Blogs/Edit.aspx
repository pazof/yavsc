<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" %>
<asp:Content ID="initContent" ContentPlaceHolderID="init" runat="server">
</asp:Content>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="overHeaderOneContent" ContentPlaceHolderID="overHeaderOne" runat="server">
</asp:Content>
<asp:Content ID="headerContent" ContentPlaceHolderID="header" runat="server">
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<h1 id="vtitle" for="Title" class="editable" ><%=Html.Markdown(Model.Title)%></h1>
<img src="<%=Model.Photo%>" alt="photo" id="vphoto" class="editable">
<div id="vcontent" for="Content" class="editable">
<%=Html.Markdown(Model.Content,"/bfiles/"+Model.Id+"/")%>
</div>

</asp:Content>
