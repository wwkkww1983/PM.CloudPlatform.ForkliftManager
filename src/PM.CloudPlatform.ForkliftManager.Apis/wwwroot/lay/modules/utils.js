/** kitadmin-v2.1.0 MIT License By http://kit.zhengjinfan.cn Author Van Zheng */ ;
"use strict";
var _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(t) {
	return typeof t
} : function(t) {
	return t && "function" == typeof Symbol && t.constructor === Symbol && t !== Symbol.prototype ? "symbol" : typeof t
};
layui.define(["lodash", "axios"], function(t) {
	var n = layui.lodash,
		o = layui.axios;
	t("utils", {
		error: function(t) {
			console.error(t)
		},
		oneOf: function(t, o) {
			var e = !1;
			return n.forEach(o, function(n, o) {
				n === t && (e = !0)
			}), e
		},
		localStorage: {
			getItem: function(t) {
				return JSON.parse(localStorage.getItem(t))
			},
			setItem: function(t, n) {
				var o = "object" === (void 0 === n ? "undefined" : _typeof(n)) || "array" == typeof n ? JSON.stringify(n) : n;
				localStorage.setItem(t, o)
			},
			removeItem: function(t) {
				localStorage.removeItem(t)
			},
			clear: function() {
				localStorage.clear()
			}
		},
		find: function(t, o) {
			return t[n.findKey(t, o)]
		},
		tplLoader: function(t, e, r) {
			var a = this,
				i = "";
			o.get(t).then(function(t) {
				var o = [],
					e = (i = t.data).match(/id=\"\w*\"/g);
				null !== e && n.forEach(e, function(t) {
					o.push(t)
				});
				var r = i.match(/lay-filter=\"\w*\"/g);
				null !== r && n.forEach(r, function(t) {
					o.push(t)
				}), o.length > 0 && n.forEach(o, function(t) {
					var n = t.match(/\"\w*\"/);
					if (void 0 !== n && null != n && n.length > 0) {
						var o = n[0],
							e = o.substring(1, o.length - 1),
							r = new RegExp(e, "g");
						i = i.replace(r, a.randomCode())
					}
				})
			}).catch(function(t) {
				var n = t.request,
					o = "读取模板出现异常，异常代码：" + n.status + "、 异常信息：" + n.statusText;
				console.log(o), "function" == typeof r && r(o)
			});
			var u = setInterval(function() {
				"" !== i && (clearInterval(u), e(i))
			}, 50)
		},
		setUrlState: function(t, n) {
			history.pushState({}, t, n)
		},
		randomCode: function() {
			return "r" + Math.random().toString(36).substr(2)
		},
		isFunction: function(t) {
			return "function" == typeof t
		},
		isString: function(t) {
			return "string" == typeof t
		},
		isObject: function(t) {
			return "object" === (void 0 === t ? "undefined" : _typeof(t))
		}
	})
});
//# sourceMappingURL=utils.js.map
