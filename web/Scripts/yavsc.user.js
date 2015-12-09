(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.user', {
    options: {
      disabled: false,
      circles: [],
      },
    buttonCircles: null,
    buttonBlog: null,
    buttonInstMsg: null,
    buttonMailling: null,
    buttonAdmin: null,
    circles: [],
    _create: function() {
       var _this = this;
       var $this = $(this);
	   var $ctl = $(this.element); 
	   if (!_this.options.disabled) {
	     var roles = $this.data('roles');
	     var bcounter = $this.data('blog-counter');
	     var circlesspec = $this.data('circles');
	     console.log('here');
	     if (bcounter)
	        if (bcounter>0) {
	   	 		_this.buttonBlog = $('<a><i class="fa fa-folder"></i></a>');
	   	 		$ctl.append(_this.buttonBlog);
	   	 		}

	   	 if (circlesspec)
	   	   {
	   	    _this.circles = circlesspec.split(' ');
	   	    }

	   	    var text = '<form><fieldset class="mayhide"><i class="fa fa-users"></i></fieldset>\n';
	   	    for (i = 0; i < _this.options.circles.length; i++) {
	   	        var checked = _this.circles.indexOf(_this.options.circles[i])>-1;
	   	        if (checked) checked = " checked";
    			text += "<input type='checkbox'"+checked+">"+ _this.options.circles[i] + "</option>\n";
			}
			text += "</form>";
			_this.buttonCircles = $(text);
			$ctl.append(_this.buttonCircles);
			return  $ctl;
	   }
	},
})})(jQuery);
}).call(this);

