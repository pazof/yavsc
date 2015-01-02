<%@ Page Title="Devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Estimate>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="head" ID="head1" runat="server" >
<script type="text/javascript" src="<%=Url.Content("~/Scripts/jquery-2.1.1.js")%>"></script> 
<script type="text/javascript" src="<%=Url.Content("~/Scripts/jquery.tablesorter.js")%>"></script> 
<script type="text/javascript" src="<%=Url.Content("~/Scripts/jquery.validate.js")%>"></script> 
<link rel="stylesheet" href="<%=Url.Content("~/Theme/dark/style.css")%>" type="text/css" media="print, projection, screen" />
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Devis") %>
<% using  (Html.BeginForm("Estimate","FrontOffice")) { %>
<%= Html.LabelFor(model => model.Title) %>:<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.Hidden ("Responsible") %>
<%= Html.LabelFor(model => model.Client) %>:<%=Html.TextBox( "Client" ) %>
<%= Html.ValidationMessage("Client", "*") %>
<br/>
<%= Html.LabelFor(model => model.Description) %>:<%=Html.TextArea( "Description") %>
<%= Html.ValidationMessage("Description", "*") %>
<br/>
<%= Html.Hidden( "Id" ) %>
<br/>
<% if (Model.Id==0) { %>
   <input type="submit" name="submit" value="Create"/>
<% } else { %>
   <input type="submit" name="submit" value="Update"/>
<% } %>


   <% if (Model.Id>0) { %>
<table class="tablesorter">
<thead>
<tr>
<th><%=Yavsc.Model.LocalizedText.Description%></th>
<th><%=Yavsc.Model.LocalizedText.Product_reference%></th>
<th><%=Yavsc.Model.LocalizedText.Count%></th>
<th><%=Yavsc.Model.LocalizedText.Unitary_cost%></th>
</tr>
</thead>
<tbody id="wrts">
<% int lc=0;
   if (Model.Lines!=null)
   foreach (Writting wr in Model.Lines) { lc++; %>
<tr class="<%= (wr.Id%2==0)?"even ":"odd " %>row" id="wr<%=wr.Id%>">
<td><%=wr.Description%></td>
<td><%=wr.ProductReference%></td>
<td><%=wr.Count%></td>
<td><%=wr.UnitaryCost%></td>
</tr>
<%    } %>
</tbody>
</table>
<%  } %>
<% } %>
</asp:Content>
   <asp:Content ContentPlaceHolderID="MASContent" ID="MASContent1" runat="server">


   <% ViewData["EstimateId"]=Model.Id; %>
   <%= Html.Partial("Writting",new Writting(),new ViewDataDictionary(ViewData)
        {
            TemplateInfo = new System.Web.Mvc.TemplateInfo
            {
                HtmlFieldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix==""?"wr":ViewData.TemplateInfo.HtmlFieldPrefix+"_wr"
            }
        }) %>

       
        <form>
     <div>
        <input type="button" id="btnnew" value="Nouvelle écriture" class="actionlink"/>
        <input type="button" id="btncreate" value="Ecrire" class="actionlink"/>
        <input type="button" id="btnmodify" value="Modifier" class="hidden actionlink"/>
        <input type="button" id="btndrop" value="Supprimer" class="hidden actionlink"/>
      </div>  </form>
     <tt id="msg" class="hidden message"></tt>

     <style>
     .row { cursor:pointer; }
     table.tablesorter td:hover { background-color: rgba(0,64,0,0.5); }
     .hidden { display:none; }
     .selected td { background-color: rgba(0,64,0,0.5); }
</style>
     <script>

 jQuery.support.cors = true;  
 function message(msg) { 
  if (msg) { 
  $("#msg").removeClass("hidden");
  $("#msg").text(msg);
  } else { $("#msg").addClass("hidden"); } }

  function GetWritting () {
     	return {
     	Id: Number($("#wr_Id").val()),
     	UnitaryCost: Number($("#wr_UnitaryCost").val()),
     	Count: parseInt($("#wr_Count").val()),
     	ProductReference: $("#wr_ProductReference").val(),
     	Description: $("#wr_Description").val()
     	};
     }

     function wredit(pwrid)
     {
        if (wr_Id.value>0) {
     	$("#wr"+wr_Id.value).removeClass("selected");
     	$("#wr"+wr_Id.value).addClass((wr_Id.value%2==0)?"even":"odd");
     	}
     	$("#wr_Id").val(pwrid);
     	if (pwrid>0) {
    	$("#btncreate").addClass("hidden");
    	$("#btnmodify").removeClass("hidden");
    	$("#btndrop").removeClass("hidden");
    	$("#wr"+wr_Id.value).removeClass((wr_Id.value%2==0)?"even":"odd");
    	$("#wr"+wr_Id.value).addClass("selected");
    	} else {
    	$("#btncreate").removeClass("hidden");
    	$("#btnmodify").addClass("hidden");
    	$("#btndrop").addClass("hidden");
    	}
     } 

     function delRow() {
       $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/DropWritting")%>",
            type: "Get",
            data: { wrid: wr_Id.value }, 
            contentType: 'application/json; charset=utf-8',
            success: function () { 
		       $("#wr"+wr_Id.value).remove();
		       $("#wr_Id").val(0);
		    	$("#wr_UnitaryCost").val(0);
		    	$("#wr_Count").val(0);
		    	$("#wr_Description").val();
		    	$("#wr_ProductReference").val();
			   wredit(0);
				message(false);
            },
            error: function (xhr, ajaxOptions, thrownError) {
        message(xhr.status+" : "+xhr.responseText);}
        });

      }
	
	function setRow() {
       var wrt = GetWritting();
       $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/UpdateWritting")%>",
            type: 'POST',
            data: wrt, 
            success: function () { 
		       var cells = document.getElementById("wr"+wrt.Id).getElementsByTagName("TD");
		       cells[0].innerHTML=wrt.Description;
		       cells[1].innerHTML=wrt.ProductReference;
		       cells[2].innerHTML=wrt.Count;
		       cells[3].innerHTML=wrt.UnitaryCost;
			   message(false);
            },
            error: function (xhr, ajaxOptions, thrownError) {
            message (xhr.status+" : "+xhr.responseText+" / "+thrownError);}
        });
    }


function addRow(){
       var wrt = GetWritting();
		var estid = parseInt($("#Id").val());
        
            $("#Err_wr_Description").text("");
            $("#Err_wr_ProductReference").text("");
            $("#Err_wr_UnitaryCost").text("");
            $("#Err_wr_Count").text("");

        $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/Write?estid=")%>"+estid,
            type: "POST",
            data: wrt,
            success: function (data) { 
            wrt.Id = Number(data);
             wredit(wrt.Id);
            var wridval = 'wr'+wrt.Id;
            jQuery('<tr/>', { 
            id: wridval,
            "class": 'selected row',
            }).appendTo('#wrts');
            $("<td>"+wrt.Description+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.ProductReference+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.Count+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.UnitaryCost+"</td>").appendTo("#"+wridval);
            $("#"+wridval).click(function(ev){onEditRow(ev);});
     $(".tablesorter").tablesorter( {sortList: [[0,0], [1,0]]} );
            message(false);
           
            },
            dataType: "json",
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		document.getElementById("Err_" + value.key.replace(".","_")).innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
			if (xhr.status != 400)
            	message(xhr.status+" : "+xhr.responseText+" / "+thrownError);}});
    }

     function onEditRow(e) {
    	var cells = e.delegateTarget.getElementsByTagName("TD");
    	var hid=e.delegateTarget.id;
    	var vwrid = Number(hid.substr(2));

    	wredit(vwrid);
    	$("#wr_Description").val(cells[0].innerHTML);
    	$("#wr_ProductReference").val(cells[1].innerHTML);
    	$("#wr_Count").val(cells[2].innerHTML);
    	$("#wr_UnitaryCost").val(Number(cells[3].innerHTML.replace(",",".")));
    
    }
  
 $(document).ready(function () {
     // bug when no row: 
     <% if (Model.Lines != null) if (Model.Lines.Length>0) { %>
     $(".tablesorter").tablesorter( {sortList: [[0,0], [1,0]]} );
     <% } %> 
        
    $("#btncreate").click(addRow);
    $("#btnmodify").click(setRow);
    $("#btndrop").click(delRow);
    $(".row").click(function (e) {onEditRow(e);});
	$("#btnnew").click(function () {
		wredit(0);
		$("#wr_Description").val("");
    	$("#wr_ProductReference").val("");
    	$("#wr_Count").val(1);
    	$("#wr_UnitaryCost").val(0);
	});
});
    </script>
  
   
    <div>
    <a class="actionlink" href="<%=ViewData["WebApiBase"]%>/FrontOffice/GetEstimTex?estimid=<%=Model.Id%>"><%= LocalizedText.Tex_version %></a>
    <a class="actionlink" href="<%=ViewData["WebApiBase"]%>/FrontOffice/GetEstimPdf?estimid=<%=Model.Id%>"><%= LocalizedText.Pdf_version %></a>
    </div>
</asp:Content>


