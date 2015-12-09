(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.hidefieldset', {
    options: {
      jsCallBack: null,
      disabled: false,
      classOpen: 'fa-minus',
      classClosed: 'fa-plus',
      },
    button: null,
    _create: function() {
	   var $ctl = $(this.element); 
	   var _this = this;
	   var _btn = $ctl.children('legend');
	   if (!_this.options.disabled && _btn) {
	        _btn.css('cursor','pointer');
	        _btn.addClass('fa');
	        _btn.addClass(_this.options.classClosed);
	        _this.button = _btn;
	        $ctl.children(':not(legend)').hide();
	        var onactivate = function (e) {
		   		if ( _btn.hasClass(_this.options.classClosed) )  {
			        _btn.removeClass(_this.options.classClosed);
			        _btn.addClass(_this.options.classOpen);
			   		$ctl.children(':not(legend)').show();
		   		}
			   	else {
			        _btn.addClass(_this.options.classClosed);
			        _btn.removeClass(_this.options.classOpen);
			   		$ctl.children(':not(legend)').hide();

			   	}
	   		};
	   		_this.button.click(onactivate);
	   		//_this.button.hover(onactivate,onactivate);
	   		//_this.click(onactivate);
	   		//_this.hover(onactivate,onactivate);
	   }
	},
})})(jQuery);
}).call(this);

