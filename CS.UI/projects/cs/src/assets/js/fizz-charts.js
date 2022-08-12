"use strict";

var _interopRequireDefault = require("@babel/runtime/helpers/interopRequireDefault");

var _toConsumableArray2 = _interopRequireDefault(require("@babel/runtime/helpers/toConsumableArray"));

var _defineProperty2 = _interopRequireDefault(require("@babel/runtime/helpers/defineProperty"));

var _possibleConstructorReturn2 = _interopRequireDefault(require("@babel/runtime/helpers/possibleConstructorReturn"));

var _getPrototypeOf2 = _interopRequireDefault(require("@babel/runtime/helpers/getPrototypeOf"));

var _get2 = _interopRequireDefault(require("@babel/runtime/helpers/get"));

var _inherits2 = _interopRequireDefault(require("@babel/runtime/helpers/inherits"));

var _regenerator = _interopRequireDefault(require("@babel/runtime/regenerator"));

var _asyncToGenerator2 = _interopRequireDefault(require("@babel/runtime/helpers/asyncToGenerator"));

var _slicedToArray2 = _interopRequireDefault(require("@babel/runtime/helpers/slicedToArray"));

var _classCallCheck2 = _interopRequireDefault(require("@babel/runtime/helpers/classCallCheck"));

var _createClass2 = _interopRequireDefault(require("@babel/runtime/helpers/createClass"));

"use strict";

var fizz_base_url = ".";
var svgns = "http://www.w3.org/2000/svg";
var xlinkns = "http://www.w3.org/1999/xlink";

export var FizzChart =
/*#__PURE__*/
function () {
  function FizzChart(data, data_format, containers, facet_options, options) {
    (0, _classCallCheck2.default)(this, FizzChart);
    this.chart_templates = null;
    this.data = data;
    this.data_format = data_format || "application/json";
    this.container = containers.chart || document.getElementById("fizz_chart");
    this.table = containers.table;
    this.model = null;
    this.root = null;
    this.el = null;
    this.defs_el = null;
    this.style_el = null;
    this.stylesheet = null;
    this.frame = null;
    this.axes = {
      x: null,
      y: null
    };
    this.chart = null;
    this.charts = {};
    this.max_chars_y_tick = null;
    this.max_chars_x_tick = null;
    this.bbox_exclusions = [];
    this.dictionary = new Dictionary();
    this.colors = new Colors();
    this.symbols = new SymbolLibrary(this);
    this.patterns = new PatternLibrary(this);
    this.gradients = new GradientLibrary(this);
    this.facet_options = facet_options || {};
    this.import_options(options);
    this.init(options);
  }

  (0, _createClass2.default)(FizzChart, [{
    key: "init",
    value: function init(options) {
      this.chart_templates = {
        'xy': {
          'class': XYChart,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'bar': {
          'class': BarChart,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'line': {
          'class': LineChart,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'area': {
          'class': AreaChart,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'scatterplot': {
          'class': ScatterPlot,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'histogram': {
          'class': Histogram,
          'base': XYChart,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'radial': {
          'class': RadialChart,
          'base': RadialChart,
          'axes': [{
            'segment': 'record'
          }]
        },
        'pie': {
          'class': PieChart,
          'base': RadialChart,
          'axes': [{
            'segment': 'record'
          }]
        },
        'donut': {
          'class': DonutChart,
          'base': RadialChart,
          'axes': [{
            'segment': 'record'
          }]
        },
        'gauge': {
          'class': GaugeChart,
          'base': RadialChart,
          'axes': [{
            'segment': 'record'
          }]
        },
        'table': {
          'class': Table,
          'base': Table,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        },
        'heatmap': {
          'class': Table,
          'base': Table,
          'axes': [{
            'x': 'facet'
          }, {
            'y': 'facet'
          }]
        }
      };
      this.root = document.createElementNS(svgns, "svg");
      this.root.setAttribute("id", "fizz_chart-".concat(this.chart_type));
      this.root.setAttribute("xmlns", svgns);
      this.root.setAttribute("xmlns:xlink", xlinkns);
      this.root.setAttribute("aria-charttype", this.chart_type);
      this.root.setAttribute("tabindex", "0");
      this.container.appendChild(this.root);
      this.update_viewbox();

      if ("application/json" === this.data_format) {
        this.create_model_from_JSON();
      } else if ("text/csv" === this.data_format) {
        var json_data = this.convert_CSV_to_JSON(this.data);
        this.create_model_from_JSON(json_data);
      }
    }
  }, {
    key: "import_options",
    value: function import_options(options) {
      this.options = options || FizzSettings.options;
      this.chart_type = this.options.chart.chart_type;
      this.multiseries = this.options.multiseries.type || "single";
      this.area = {
        x: this.options.chart.area.x || 0,
        y: this.options.chart.area.y || 0,
        width: this.options.chart.area.width || 500,
        height: this.options.chart.area.height || 500
      };
      this.colors.set_colors(this.options.colors);
    }
  }, {
    key: "update_viewbox",
    value: function update_viewbox(x, y, width, height) {
      var viewbox_x = x || this.area.x;
      var viewbox_y = y || this.area.y;
      var viewbox_width = width || this.area.width;
      var viewbox_height = height || this.area.height;
      this.root.setAttribute("viewBox", "".concat(viewbox_x, " ").concat(viewbox_y, " ").concat(viewbox_width, " ").concat(viewbox_height));

      if (!this.frame) {
        this.frame = document.createElementNS(svgns, "rect");
        this.frame.setAttribute("id", "fizz_chart_frame");
        this.frame.setAttribute("fill", "none");
        this.frame.setAttribute("pointer-events", "all");
        this.root.appendChild(this.frame);
      }

      this.frame.setAttribute("x", viewbox_x);
      this.frame.setAttribute("y", viewbox_y);
      this.frame.setAttribute("width", viewbox_width);
      this.frame.setAttribute("height", viewbox_height);
    }
  }, {
    key: "update_defs",
    value: function update_defs(el) {
      if (!this.defs_el) {
        this.defs_el = document.createElementNS(svgns, "defs");
        this.defs_el.id = "fizz-defs";
        this.defs_el.appendChild(document.createTextNode(" "));
        var title_el = this.root.querySelector("title");
        this.root.insertBefore(this.defs_el, title_el.nextSibling);
      }

      this.defs_el.appendChild(el);

      if (!this.defs_el.firstElementChild) {
        this.defs_el.remove();
      }
    }
  }, {
    key: "insert_styles",
    value: function insert_styles(json) {
      var _this = this;

      if (this.style_el) {
        this.style_el.textContent = " ";
      } else {
        this.style_el = document.createElementNS(svgns, "style");

        if (!this.style_el.sheet) {
          this.style_el = document.createElement("style");
        }

        this.style_el.id = "fizz-styles";
        this.style_el.appendChild(document.createTextNode(" "));
        this.root.appendChild(this.style_el);
      }

      this.stylesheet = this.style_el.sheet;
      var selectors = Object.keys(json);
      var styles = selectors.map(function (selector) {
        var definition = json[selector];
        var rules = Object.keys(definition);
        var result = rules.map(function (property) {
          var statement = "".concat(property, ": ").concat(definition[property]);
          return statement;
        }).join(";\n");
        var each_rule = "".concat(selector, " { ").concat(result, " }");

        _this.stylesheet.insertRule(each_rule, _this.stylesheet.cssRules.length);

        _this.style_el.appendChild(document.createTextNode(each_rule));

        return each_rule;
      }).join('\n');
    }
  }, {
    key: "import_styles",
    value: function import_styles(src_style_el, remove_src) {
      if (this.stylesheet && src_style_el) {
        var src_stylesheet = src_style_el.sheet;
        var rule_count = src_stylesheet.cssRules.length;

        for (var r = 0; rule_count > r; ++r) {
          var each_rule = src_stylesheet.cssRules[r].cssText;
          this.stylesheet.insertRule(each_rule, this.stylesheet.cssRules.length);
          this.style_el.appendChild(document.createTextNode(each_rule));
        }

        if (remove_src) {
          src_style_el.remove();
        }
      }
    }
  }, {
    key: "add_style_rule",
    value: function add_style_rule(selector, properties, override) {
      var rule = null;
      var rule_count = this.stylesheet.cssRules.length;

      for (var r = 0; rule_count > r; ++r) {
        var each_rule = this.stylesheet.cssRules[r];

        if (each_rule.selectorText === selector) {
          rule = each_rule;
          break;
        }
      }

      if (!rule || true === override) {
        var declaration = "";
        Object.entries(properties).forEach(function (_ref) {
          var _ref2 = (0, _slicedToArray2.default)(_ref, 2),
              key = _ref2[0],
              value = _ref2[1];

          return declaration += "".concat(key, ": ").concat(value, "; ");
        });
        var new_rule = "".concat(selector, " { ").concat(declaration, " }");
        this.stylesheet.insertRule(new_rule, rule_count);
        this.style_el.appendChild(document.createTextNode(new_rule));
      }
    }
  }, {
    key: "add_chart_styles",
    value: function add_chart_styles(styles) {
      var _this2 = this;

      Object.entries(styles.series).forEach(function (_ref3) {
        var _ref4 = (0, _slicedToArray2.default)(_ref3, 2),
            id = _ref4[0],
            fill_style = _ref4[1];

        var darker = _this2.colors.generate_sequential_palette(fill_style.color, 2, false)[1];

        var lighter = _this2.colors.generate_sequential_palette(fill_style.color, 2, true)[1];

        var fill = fill_style.color;

        if (fill_style.paint_ref && !styles.classes.includes("data_symbol")) {
          fill = "url(".concat(window.location.href, "#").concat(fill_style.paint_ref, ")");
        }

        _this2.add_style_rule(".".concat(id), {
          "fill": fill,
          "stroke": fill
        });

        _this2.add_style_rule(".datapoint_background:hover,\n      .datapoint_background:focus", {
          "outline": "2px auto -webkit-focus-ring-color",
          "box-shadow": "none"
        });

        _this2.add_style_rule(".".concat(id, ":hover path:not(.data_line),\n      .").concat(id, ":focus path:not(.data_line)"), {
          "fill": lighter,
          "outline": "2px auto -webkit-focus-ring-color",
          "box-shadow": "none"
        });

        if (styles.classes.includes("data_line")) {
          _this2.add_style_rule(".".concat(id, " .data_line"), {
            "fill": "none",
            "stroke": fill_style.color
          });
        }

        if (styles.classes.includes("data_area")) {
          _this2.add_style_rule(".".concat(id, " .data_area"), {
            "fill": fill,
            "stroke": "none",
            "fill-opacity": "0.8"
          });
        }
      });
    }
  }, {
    key: "convert_CSV_to_JSON",
    value: function convert_CSV_to_JSON(csv_data) {
      var lines = csv_data.split("\n");
      var keys = lines[0].split(",");
      var json_data = [];

      for (var l = 1, l_len = lines.length; l_len > l; ++l) {
        var entries = lines[l].split(",");

        if (lines[l] && lines[l].length) {
          var _entries = lines[l].split(",");

          var each_record = {};

          for (var e = 0, e_len = _entries.length; e_len > e; ++e) {
            each_record[keys[e]] = _entries[e];
          }

          json_data.push(each_record);
        }
      }

      return json_data;
    }
  }, {
    key: "convert_table_to_JSON",
    value: function convert_table_to_JSON(table) {
      var json_data = null;
      create_model_from_JSON(json_data);
    }
  }, {
    key: "create_model_from_JSON",
    value: function () {
      var _create_model_from_JSON = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee(json_data) {
        var d, d_len, record, key, val, datapoint;
        return _regenerator.default.wrap(function _callee$(_context) {
          while (1) {
            switch (_context.prev = _context.next) {
              case 0:
                if (!json_data) {
                  json_data = this.data;
                }

                this.model = {
                  keys: [],
                  data: [],
                  records: [],
                  facets: {}
                };

                for (d = 0, d_len = json_data.length; d_len > d; ++d) {
                  record = new DataRecord(d);

                  for (key in json_data[d]) {
                    this.add_key_to_data_model(key);
                    val = json_data[d][key];
                    datapoint = new DataPoint(key, d, val, record.id);
                    this.model.facets[key].datapoints.push(datapoint);

                    if ("numeric" !== datapoint.datatype) {
                      datapoint.value.format = datapoint.value.norm;

                      if (-1 === this.model.facets[key].categories.indexOf(datapoint.value.raw)) {
                        this.model.facets[key].categories.push(datapoint.value.raw);
                      }

                      if (this.model.facets[key].max_chars < datapoint.value.raw.length) {
                        this.model.facets[key].max_chars = datapoint.value.raw.length;
                      }
                    } else {
                      datapoint.value.format = utils.format_number(datapoint.value.norm);
                    }

                    record.datapoints[key] = datapoint;
                  }

                  this.model.records.push(record);
                }

                if (1 === this.model.keys.length) {
                  this.generate_frequency_data(this.model.keys[0]);
                }

                this.set_facet_datatypes();
                this.generate_bins();
                _context.prev = 6;
                _context.next = 9;
                return this.match_key_vocabs();

              case 9:
                _context.next = 11;
                return this.set_vocabs();

              case 11:
                _context.next = 16;
                break;

              case 13:
                _context.prev = 13;
                _context.t0 = _context["catch"](6);
                throw new Error("create_model_from_JSON(): vocab loading", error);

              case 16:
                _context.prev = 16;
                return _context.finish(16);

              case 18:
                this.draw();

              case 19:
              case "end":
                return _context.stop();
            }
          }
        }, _callee, this, [[6, 13, 16, 18]]);
      }));

      function create_model_from_JSON(_x) {
        return _create_model_from_JSON.apply(this, arguments);
      }

      return create_model_from_JSON;
    }()
  }, {
    key: "add_key_to_data_model",
    value: function add_key_to_data_model(key) {
      if (-1 === this.model.keys.indexOf(key)) {
        this.model.keys.push(key);
        var facet = new Facet(key, this.chart_type);
        this.model.facets[key] = facet;
        this.colors.register_key(key);
        var color_key = this.colors.keys.get(key);
        color_key.id = facet.id;
        color_key.base = this.colors.palette[color_key.index];
        facet.color = color_key.base;
      }
    }
  }, {
    key: "get_data_model",
    value: function get_data_model() {
      return this.model;
    }
  }, {
    key: "set_facet_options",
    value: function set_facet_options() {
      var _this3 = this;

      Object.keys(this.facet_options).forEach(function (key) {
        var each_facet_option = _this3.facet_options[key];
        var each_facet = _this3.model.facets[key];

        if (each_facet) {
          Object.keys(each_facet_option).forEach(function (key) {
            var value = each_facet_option[key];

            if ("selected" === key) {
              if (false === value) {
                _this3.select_facet(each_facet.key, false, null);
              } else {
                _this3.select_facet(each_facet.key, true, each_facet_option.axis);
              }
            } else if ("color" === key || "secondary_color" === key) {
              var hsl = value;

              if (value.includes("#")) {
                hsl = _this3.colors.hex_to_hsl(value, true);
              }

              each_facet[key] = hsl;
            } else if ("symbol_id" === key) {} else if ("pattern_type" === key) {
              each_facet[key] = value;
            } else if ("axis" !== key) {
              each_facet[key] = value;
            }
          });
        }
      });
    }
  }, {
    key: "generate_frequency_data",
    value: function generate_frequency_data(key) {
      var values = datatools.get_norm_values(this.model.facets[key]);
      var bounds = utils.get_numeric_bounds(values, "x", this.options.axis);
      this.model.facets[key].bounds = bounds;
      var frequency_key = "".concat(key, " frequency");

      if (1 === this.model.keys.length) {
        frequency_key = "frequency";
        this.model.keys[0] = frequency_key;
        this.model.keys[1] = key;
      }

      this.add_key_to_data_model(frequency_key);
      var frequency = [];

      for (var i = 0; i < bounds.interval; i++) {
        frequency[i] = 0;
      }

      for (var _i = 0; _i < values.length; _i++) {
        var bin_interval = bounds.label_min + bounds.mult;
        var bin_count = 0;

        while (bin_interval <= bounds.label_max) {
          if (values[_i] < bin_interval) {
            frequency[bin_count]++;
            break;
          } else {
            bin_interval += bounds.mult;
            bin_count++;
          }
        }
      }

      this.model.facets[key].norm_values = bounds.tick_label_array.slice(0, bounds.tick_label_array.length - 1);

      for (var _i2 = 0; _i2 < frequency.length; _i2++) {
        var val = frequency[_i2];
        var datapoint = new DataPoint(frequency_key, _i2, val);
        this.model.facets[key].datapoints.push(datapoint);
      }
    }
  }, {
    key: "set_facet_datatypes",
    value: function set_facet_datatypes() {
      for (var _i3 = 0, _Object$keys = Object.keys(this.model.facets); _i3 < _Object$keys.length; _i3++) {
        var key = _Object$keys[_i3];

        if (this.facet_options && this.facet_options[key] && this.facet_options[key].datatype) {
          this.model.facets[key].datatype = this.facet_options[key].datatype;
        } else {
          var datatype_array = this.model.facets[key].datapoints.map(function (datapoint) {
            return datapoint.datatype;
          });

          if (utils.has_uniform_values(datatype_array)) {
            this.model.facets[key].datatype = datatype_array[0];
          } else {}
        }
      }
    }
  }, {
    key: "match_key_vocabs",
    value: function () {
      var _match_key_vocabs = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee2() {
        var key, vocabs_loaded;
        return _regenerator.default.wrap(function _callee2$(_context2) {
          while (1) {
            switch (_context2.prev = _context2.next) {
              case 0:
                for (key in this.model.facets) {
                  if (this.model.facets.hasOwnProperty(key)) {
                    this.dictionary.find_vocabs(key);
                  }
                }

                vocabs_loaded = this.dictionary.load_vocabs();
                vocabs_loaded.then(function (success) {
                  return true;
                }).catch(function (error) {
                  return false;
                });
                return _context2.abrupt("return", vocabs_loaded);

              case 4:
              case "end":
                return _context2.stop();
            }
          }
        }, _callee2, this);
      }));

      function match_key_vocabs() {
        return _match_key_vocabs.apply(this, arguments);
      }

      return match_key_vocabs;
    }()
  }, {
    key: "set_vocabs",
    value: function () {
      var _set_vocabs = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee3() {
        var k, k_len, key, facet, first_val, key_map, default_key;
        return _regenerator.default.wrap(function _callee3$(_context3) {
          while (1) {
            switch (_context3.prev = _context3.next) {
              case 0:
                for (k = 0, k_len = this.model.keys.length; k_len > k; ++k) {
                  key = this.model.keys[k];
                  facet = this.model.facets[key];

                  if ("numeric" === facet.datatype) {}

                  first_val = this.model.facets[key].datapoints[0].value.norm;
                  key_map = this.dictionary.map_key_to_vocab(key, first_val);
                  this.model.facets[key].label_map = key_map;

                  if (this.model.facets[key].label_map) {
                    if (-1 != this.model.facets[key].label_map.alternative_keys.indexOf("abbreviation")) {
                      this.model.facets[key].label_map.label_key = "abbreviation";
                    }

                    default_key = this.model.facets[key].label_map.default_key;
                    this.model.facets[key].label_map.lookup = this.dictionary.create_vocab_lookup_array(key, default_key);
                  }
                }

              case 1:
              case "end":
                return _context3.stop();
            }
          }
        }, _callee3, this);
      }));

      function set_vocabs() {
        return _set_vocabs.apply(this, arguments);
      }

      return set_vocabs;
    }()
  }, {
    key: "detect_date",
    value: function detect_date(str) {
      var MMDDYYY = /^(?:(?:(?:0?[13578]|10|12)(-|\/)(?:0?[1-9]|[12]\d?|3[01]?)\1|(?:0?[469]|11)(-|\/)(?:0?[1-9]|[12]\d?|3[0]?)\2|0?2(-|\/)(?:0?[1-9]|1\d|2[0-8])\3)(?:19[2-9]\d{1}|20[01]\d{1}|\d{2}))$/;
      var YYYYMMDD = /^(?:(?:(?:(?:(?:[1-9]\d)(?:0[48]|[2468][048]|[13579][26])|(?:(?:[2468][048]|[13579][26])00))(\/|-|\.)(?:0?2\1(?:29)))|(?:(?:[1-9]\d{3})(\/|-|\.)(?:(?:(?:0?[13578]|1[02])\2(?:31))|(?:(?:0?[13-9]|1[0-2])\2(?:29|30))|(?:(?:0?[1-9])|(?:1[0-2]))\2(?:0?[1-9]|1\d|2[0-8])))))$/;
      var is_date = false;

      if (str.match(MMDDYYY) || str.match(YYYYMMDD)) {
        is_date = true;
      }

      return is_date;
    }
  }, {
    key: "generate_bins",
    value: function generate_bins(key) {
      for (var _i4 = 0, _Object$keys2 = Object.keys(this.model.facets); _i4 < _Object$keys2.length; _i4++) {
        var _key = _Object$keys2[_i4];

        if (this.facet_options[_key] && this.facet_options[_key].bin_details) {
          this.model.facets[_key].bin_details = this.facet_options[_key].bin_details;
        } else {
          var facet = this.model.facets[_key];
          this.determine_default_bin_count(_key);
          this.set_facet_bins(_key);
        }
      }
    }
  }, {
    key: "determine_default_bin_count",
    value: function determine_default_bin_count(key) {
      var facet = this.model.facets[key];
      var bin_details = facet.bin_details;

      if (bin_details) {
        return bin_details.count;
      }

      var datatype = facet.datatype;
      facet.bin_details = {};
      var default_bin_count = 2;

      if ("category" === datatype || "location" === datatype || "date" === datatype) {
        default_bin_count = facet.categories.length;
      }

      facet.bin_details.count = default_bin_count;
    }
  }, {
    key: "select_facet",
    value: function select_facet(key, is_selected, axis, refresh) {
      var facet = this.model.facets[key];

      if (facet && undefined !== is_selected) {
        facet.selected = is_selected;
      }

      if (undefined !== axis) {
        facet.axis = axis;
      }

      if (refresh) {}
    }
  }, {
    key: "set_facet_bins",
    value: function set_facet_bins(key) {
      var facet = this.model.facets[key];
      var datatype = facet.datatype;
      var breakpoints = this.get_bin_breakpoints(key);
      var bin_details = facet.bin_details;
      var bin_count = bin_details.count;
      var palette = this.colors.palette;

      if ("numeric" === datatype) {
        this.colors.generate_sequential_palette(facet.color, breakpoints.length, false, key);
        palette = this.colors.get_palettes([key])[0].colors;
      }

      bin_details.bins = [];

      for (var b = 0, b_len = breakpoints.length; b_len > b; ++b) {
        var each_breakpoint = breakpoints[b];

        if ("" === each_breakpoint) {
          each_breakpoint = "n/a";
        }

        var each_bin = {};

        if ("numeric" === datatype) {
          var min = each_breakpoint;
          var max = null;
          var label = null;
          var next_breakpoint = breakpoints[b + 1];

          if (undefined == next_breakpoint) {
            var norm_values = datatools.get_norm_values(facet);
            max = Math.max.apply(Math, norm_values);
            label = "".concat(utils.format_number(min), "+");
          } else {
            var p = 1;

            if (next_breakpoint.toString().includes(".")) {
              p /= 10;
            }

            max = next_breakpoint - p;
            label = "".concat(utils.format_number(min), "-").concat(utils.format_number(max));
          }

          each_bin.min = min;
          each_bin.max = max;
          each_bin.label = label;
        } else {
          each_bin.key = each_breakpoint;
          each_bin.label = each_breakpoint;
        }

        each_bin.color = palette[b];
        bin_details.bins.push(each_bin);
      }

      bin_details.palette = palette;
    }
  }, {
    key: "get_bin_breakpoints",
    value: function get_bin_breakpoints(key) {
      var facet = this.model.facets[key];
      var datatype = facet.datatype;
      var breakpoints = [];

      if ("category" === datatype || "location" === datatype || "date" === datatype) {
        breakpoints = facet.categories;
      } else if ("numeric" === datatype) {
        var bin_count = facet.bin_details.count;
        var values_array = datatools.get_norm_values(facet);
        var item_count = values_array.length;
        var total_value = values_array.reduce(function (a, b) {
          return a + b;
        });
        values_array.sort(function (a, b) {
          return a - b;
        });
        var min = values_array[0];
        var max = values_array[values_array.length - 1];
        var range = max - min;
        breakpoints.push(0);
        var total = 100;

        for (var b = 1; bin_count > b; ++b) {
          var percentile = 100 / (bin_count / b);
          var breakpoint = this.get_quantile(values_array, percentile);
          breakpoints.push(breakpoint);
        }
      }

      return breakpoints;
    }
  }, {
    key: "get_quantile",
    value: function get_quantile(array, percentile) {
      var result = 0;
      var index = percentile / 100 * (array.length - 1);

      if (Math.floor(index) == index) {
        result = array[index];
      } else {
        var i = Math.floor(index);
        var fraction = index - i;
        result = array[i] + (array[i + 1] - array[i]) * fraction;
      }

      return result;
    }
  }, {
    key: "generate_table",
    value: function generate_table() {
      if (this.table) {
        var data_table = new Table(this);
        data_table.draw();

        while (this.table.lastChild) {
          this.table.removeChild(this.table.lastChild);
        }

        this.table.appendChild(data_table.el);
      }
    }
  }, {
    key: "draw",
    value: function () {
      var _draw = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee4() {
        var chart_template, is_stack;
        return _regenerator.default.wrap(function _callee4$(_context4) {
          while (1) {
            switch (_context4.prev = _context4.next) {
              case 0:
                this.reset_chart();
                this.el = document.createElementNS(svgns, "g");
                this.el.setAttribute("role", "graphics-document");
                this.el.setAttribute("aria-roledescription", "".concat(this.chart_type, " chart"));
                this.el.setAttribute("tabindex", "0");
                this.root.appendChild(this.el);
                this.insert_styles(chart_styles);

                if (this.axes.dependent) {}

                if (this.axes.independent) {}

                if (0 < Object.keys(this.facet_options).length) {
                  this.set_facet_options();
                }

                if (!this.axes.dependent || !this.axes.independent) {
                  this.assign_axes();

                  if (!this.axes.dependent || !this.axes.independent) {
                    this.get_default_axes();
                  }
                }

                chart_template = this.chart_templates[this.axes.independent[0].chart_type];

                if (chart_template) {
                  this.set_chart_title();
                  is_stack = true;

                  if (XYChart === chart_template.base) {
                    this.axes.x = new XAxis(this, this.axes.independent, null, true, true);
                    this.axes.y = new YAxis(this, this.axes.dependent, null, true, true);
                    this.model.titles.x = this.axes.x.title;
                    this.model.titles.y = this.axes.y.title;
                    this.axes.x.draw(this.axes.y.axis_line_id, is_stack);
                    this.axes.y.draw(this.axes.x.axis_line_id, is_stack);
                  } else if (RadialChart === chart_template.base) {
                    this.axes.r = new RadialAxis(this, this.axes.dependent, null, true, true);
                    this.axes.r.draw(this.axes.r.axis_line_id, is_stack);
                  }

                  this.generate_table();
                }

              case 13:
              case "end":
                return _context4.stop();
            }
          }
        }, _callee4, this);
      }));

      function draw() {
        return _draw.apply(this, arguments);
      }

      return draw;
    }()
  }, {
    key: "reset_chart",
    value: function reset_chart() {
      var _this4 = this;

      this.model.keys.forEach(function (key) {
        return _this4.select_facet(key, false, null);
      });
      this.axes.dependent = null;
      this.axes.independent = null;

      if (this.el) {
        this.el.remove();
        this.el = null;
      }
    }
  }, {
    key: "set_chart_title",
    value: function set_chart_title() {
      this.title = this.options.chart.title.text;

      if (!this.title) {
        var dependent_str = utils.compose_title_from_facets(this.axes.dependent);
        var independent_str = utils.compose_title_from_facets(this.axes.independent);
        this.title = "".concat(dependent_str, " by ").concat(independent_str);
      }

      var chart_title_el = this.el.parentNode.querySelector("title");

      if (!chart_title_el) {
        chart_title_el = document.createElementNS(svgns, "title");
        this.el.parentNode.insertBefore(chart_title_el, this.el.parentNode.firstChild);
      }

      chart_title_el.textContent = "";
      var chart_title_properties = {
        'font-size': this.options.chart.title.font_size ? "".concat(+this.options.chart.title.font_size, "px") : "1.8rem",
        'text-anchor': this.options.chart.title.align ? this.options.chart.title.align : "middle",
        'fill': this.options.chart.title.font_color ? this.options.chart.title.font_color : "black"
      };

      if (this.options.chart.title && this.options.chart.title.text_transform) {
        chart_title_properties['text-transform'] = this.options.chart.title.text_transform;
      }

      this.label = new Label(this.title, null, null, "chart_title", "heading", null, false);
      var label_x = this.area.width / 2;

      if ("start" === this.options.chart.title.align) {
        label_x = +this.options.chart.padding;
      } else if ("end" === this.options.chart.title.align) {
        label_x = this.area.width - +this.options.chart.padding;
      }

      var label_y = +this.options.chart.title.font_size * 0.8 + +this.options.chart.padding;
      this.label.el.setAttribute("transform", "translate(".concat(label_x, ",").concat(label_y, ")"));
      this.title_value_label = this.options.chart.title.value_label;

      if (this.title_value_label) {
        var axis_components = this.title_value_label.split('.');
        var title_value_label_text = axis_components.reduce(function (p, c) {
          return p && p[c] || null;
        }, this.axes);
        var title_value_label_el = document.createElementNS(svgns, "tspan");
        title_value_label_el.textContent = " (".concat(title_value_label_text, ")");
        this.label.el.appendChild(title_value_label_el);
      }

      this.add_style_rule(".chart_title", chart_title_properties, true);
      this.el.appendChild(this.label.el);
      this.model.titles = {
        chart: this.title
      };
    }
  }, {
    key: "set_color",
    value: function () {
      var _set_color = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee5(facets) {
        var _this5 = this;

        return _regenerator.default.wrap(function _callee5$(_context5) {
          while (1) {
            switch (_context5.prev = _context5.next) {
              case 0:
                facets.forEach(function (facet) {
                  _this5.colors.register_key(facet.key);

                  var color_key = _this5.colors.keys.get(facet.key);

                  color_key.id = facet.id;
                  color_key.base = _this5.colors.palette[color_key.index];
                  facet.color = color_key.base;
                });

              case 1:
              case "end":
                return _context5.stop();
            }
          }
        }, _callee5);
      }));

      function set_color(_x2) {
        return _set_color.apply(this, arguments);
      }

      return set_color;
    }()
  }, {
    key: "set_multiseries_type",
    value: function set_multiseries_type(type) {
      this.multiseries = type;
      this.draw();
    }
  }, {
    key: "select_chart_type",
    value: function select_chart_type(chart_type) {
      var _this6 = this;

      this.model.keys.forEach(function (key) {
        return _this6.set_facet_chart_type(key, chart_type, true);
      });
      this.draw();
    }
  }, {
    key: "set_facet_datatype",
    value: function set_facet_datatype(key, datatype) {
      var facet = this.model.facets[key];
      facet.datatype = datatype;
    }
  }, {
    key: "set_facet_color",
    value: function set_facet_color(key, color) {
      var facet = this.model.facets[key];

      if (color.includes("#")) {
        color = this.colors.hex_to_hsl(color, true);
      } else if (color.includes("rgb")) {}

      facet.color = color;
      this.draw();
    }
  }, {
    key: "set_facet_label",
    value: function set_facet_label(key, text) {
      var facet = this.model.facets[key];
      facet.label = text;
    }
  }, {
    key: "set_facet_bin_color",
    value: function set_facet_bin_color(key, index, color) {
      var facet = this.model.facets[key];
      facet.bin_details.palette[index] = color;
    }
  }, {
    key: "set_facet_bin_label",
    value: function set_facet_bin_label(key, index, text) {
      var facet = this.model.facets[key];
      facet.bin_details.bins[index].label = text;
    }
  }, {
    key: "set_facet_axis",
    value: function set_facet_axis(key, axis_type) {
      var facet = this.model.facets[key];
      facet.axis = axis_type;
    }
  }, {
    key: "set_facet_chart_type",
    value: function set_facet_chart_type(key, chart_type, no_refresh) {
      var facet = this.model.facets[key];
      facet.chart_type = chart_type;

      if (!no_refresh) {
        this.draw();
      }
    }
  }, {
    key: "set_facet_order",
    value: function set_facet_order(key, facet_order) {
      var facet = this.model.facets[key];
      facet.order = facet_order;
      this.draw();
    }
  }, {
    key: "set_facet_symbol",
    value: function set_facet_symbol(key, symbol_id) {
      var facet = this.model.facets[key];
      this.symbols.remove_symbol(facet.symbol_id);
      this.symbols.assign_symbol(facet, symbol_id);
      this.draw();
    }
  }, {
    key: "sort_axis",
    value: function sort_axis(x_axis_numeric, x_values, y_values) {
      if (x_axis_numeric) {
        for (var i = 0; i < x_values.length; i++) {
          for (var j = i + 1; j < x_values.length; j++) {
            if (x_values[i] > x_values[j]) {
              var temp_x = x_values[j];
              x_values[j] = x_values[i];
              x_values[i] = temp_x;
              var temp_y = y_values[j];
              y_values[j] = y_values[i];
              y_values[i] = temp_y;
            }
          }
        }

        this.axes.y.values = y_values;
        this.axes.x.values = x_values;
      } else if (y_values) {
        this.axes.y.values = y_values;
      }
    }
  }, {
    key: "assign_axes",
    value: function assign_axes() {
      var facet_keys_by_axis = datatools.group_facet_keys_by_axis(this.model.facets);

      if (facet_keys_by_axis.has("dependent")) {
        var dependent_keys = facet_keys_by_axis.get("dependent");
        this.set_axis_from_key_set(dependent_keys, "dependent");
      }

      if (facet_keys_by_axis.has("independent")) {
        var independent_keys = facet_keys_by_axis.get("independent");
        this.set_axis_from_key_set(independent_keys, "independent");
      }
    }
  }, {
    key: "get_default_axes",
    value: function get_default_axes() {
      var facet_count = this.model.keys.length;

      if (1 === facet_count) {}

      var facet_key_datatypes = datatools.group_facet_keys_by_datatype(this.model.facets);

      if (!this.axes.dependent) {
        this.set_axes_by_datatype(facet_key_datatypes, "dependent", ["numeric", "category", "location", "date"], facet_count);
      }

      if (!this.axes.independent) {
        this.set_axes_by_datatype(facet_key_datatypes, "independent", ["date", "location", "category", "numeric"], facet_count);
      }
    }
  }, {
    key: "set_axes_by_datatype",
    value: function set_axes_by_datatype(facet_key_datatypes, axis_variable, datatype_order_arr, facet_count) {
      datatools.filter_selected_facet_keys_from_datatype_map(this.model.facets, facet_key_datatypes);
      var key_set = null;
      var _iteratorNormalCompletion = true;
      var _didIteratorError = false;
      var _iteratorError = undefined;

      try {
        for (var _iterator = datatype_order_arr[Symbol.iterator](), _step; !(_iteratorNormalCompletion = (_step = _iterator.next()).done); _iteratorNormalCompletion = true) {
          var datatype = _step.value;

          if (facet_key_datatypes.has(datatype)) {
            key_set = facet_key_datatypes.get(datatype);
            break;
          }
        }
      } catch (err) {
        _didIteratorError = true;
        _iteratorError = err;
      } finally {
        try {
          if (!_iteratorNormalCompletion && _iterator.return != null) {
            _iterator.return();
          }
        } finally {
          if (_didIteratorError) {
            throw _iteratorError;
          }
        }
      }

      if (key_set) {
        this.set_axis_from_key_set(key_set, axis_variable, facet_count);
      } else {
        throw new Error("No appropriate dependent axis key found");
      }
    }
  }, {
    key: "set_axis_from_key_set",
    value: function set_axis_from_key_set(key_set, axis_variable, facet_count) {
      var key_count = this.multiseries === "single" ? 1 : key_set.length;

      if (1 < key_count && facet_count === key_count) {
        key_count--;
      }

      for (var k = 0, k_len = key_count; k_len > k; ++k) {
        var each_key = key_set[k];
        this.select_facet(each_key, true, axis_variable);

        if (!this.axes[axis_variable]) {
          this.axes[axis_variable] = [this.model.facets[each_key]];
        } else {
          this.axes[axis_variable].push(this.model.facets[each_key]);
        }

        var each_facet = this.model.facets[each_key];

        if ("numeric" === each_facet.datatype) {
          var norm_values = datatools.get_norm_values(each_facet);
          each_facet.bounds = utils.get_numeric_bounds(norm_values, each_facet.axis, this.options.axis);
        }

        if ("dependent" === axis_variable) {}
      }
    }
  }]);
  return FizzChart;
}();

var SymbolLibrary =
/*#__PURE__*/
function () {
  function SymbolLibrary(chart_obj) {
    (0, _classCallCheck2.default)(this, SymbolLibrary);
    this.chart_obj = chart_obj;
    this.symbols = {
      "symbol_circle": {
        path: "M0,-5 A5,5 0 1,1 0,5 A5,5 0 1,1 0,-5",
        facet_id: null,
        el: null
      },
      "symbol_square": {
        path: "M-5,-5 H5 V5 H-5 Z",
        facet_id: null,
        el: null
      },
      "symbol_triangle_up": {
        path: "M-5,3.33 H5 L0,-5.66 Z",
        facet_id: null,
        el: null
      },
      "symbol_diamond": {
        path: "M0,-7.1 L7.1,0 L0,7.1 L-7.1,0 Z",
        facet_id: null,
        el: null
      },
      "symbol_cross": {
        path: "M-2.1,-5 H2.1 V-2.1 H5 V2.1 H2.1 V5 H-2.1 V2.1 H-5 V-2.1 H-2.1 Z",
        facet_id: null,
        el: null
      },
      "symbol_triangle_down": {
        path: "M-5,-3.33 H5 L0,5.66 Z",
        facet_id: null,
        el: null
      }
    };
  }

  (0, _createClass2.default)(SymbolLibrary, [{
    key: "assign_symbol",
    value: function assign_symbol(facet, symbol_id) {
      var _this7 = this;

      if (symbol_id) {
        var symbol = this.symbols[symbol_id];

        if (symbol) {
          facet.symbol_id = symbol_id;
          symbol.facet_id = facet.symbol_id;
          this.insert_symbol(facet.symbol_id, symbol);
        }
      } else {
        var symbol_ids = Object.keys(this.symbols).filter(function (symbol) {
          return !_this7.symbols[symbol].facet_id;
        });

        if (symbol_ids.length) {
          facet.symbol_id = symbol_ids[0];
          var _symbol = this.symbols[facet.symbol_id];
          _symbol.facet_id = facet.symbol_id;
          this.insert_symbol(facet.symbol_id, _symbol);
        } else {
          facet.symbol_id = this.generate_symbol(facet.id);
        }
      }
    }
  }, {
    key: "insert_symbol",
    value: function insert_symbol(symbol_id, symbol) {
      symbol.el = document.createElementNS(svgns, "svg");
      symbol.el.setAttribute("id", symbol_id);
      symbol.el.setAttribute("width", "10");
      symbol.el.setAttribute("height", "10");
      symbol.el.setAttribute("viewBox", "0 0 10 10");
      symbol.el.setAttribute("overflow", "visible");
      var shape_el = document.createElementNS(svgns, "path");
      shape_el.setAttribute("d", symbol.path);
      shape_el.setAttribute("stroke-width", "3");
      symbol.el.appendChild(shape_el);
      this.chart_obj.update_defs(symbol.el);
    }
  }, {
    key: "generate_symbol",
    value: function generate_symbol(id) {
      var symbol_id = "symbol_".concat(id);
      return symbol_id;
    }
  }, {
    key: "remove_symbol",
    value: function remove_symbol(symbol_id) {
      var symbol = this.symbols[symbol_id];

      if (symbol) {
        symbol.facet_id = null;
        symbol.el.remove();
      }
    }
  }]);
  return SymbolLibrary;
}();

var PatternLibrary =
/*#__PURE__*/
function () {
  function PatternLibrary(chart_obj) {
    (0, _classCallCheck2.default)(this, PatternLibrary);
    this.chart_obj = chart_obj;
    this.patterns = {
      "pattern_diagonal_left": {
        path: "M4,0 L0,4 M8,4 L4,8",
        width: 8,
        height: 8,
        background_shade: 4,
        stroke_width: 3
      },
      "pattern_diagonal_left_mid": {
        path: "M5,0 L0,5 M10,5 L5,10",
        width: 10,
        height: 10,
        background_shade: 9,
        stroke_width: 2.5
      },
      "pattern_diagonal_left_wide": {
        path: "M6,0 L0,6 M12,6 L6,12",
        width: 12,
        height: 12,
        background_shade: 10,
        stroke_width: 2.5
      },
      "pattern_diagonal_right": {
        path: "M4,0 L8,4 M0,4 L4,8",
        width: 8,
        height: 8,
        background_shade: 4,
        stroke_width: 3
      },
      "pattern_dot": {
        path: "M4,3.5 A0.5,0.5 0 1,1 4,4.5 A0.5,0.5 0 1,1 4,3.5 Z",
        width: 8,
        height: 8,
        background_shade: 4,
        stroke_width: 3
      }
    };
    this.active_patterns = {};
  }

  (0, _createClass2.default)(PatternLibrary, [{
    key: "assign_pattern",
    value: function assign_pattern(facet) {
      if (facet.pattern_type && this.patterns[facet.pattern_type]) {
        facet.paint_ref = "".concat(facet.pattern_type, "-").concat(facet.id);
        var template = this.patterns[facet.pattern_type];

        if (!facet.secondary_color) {
          facet.secondary_color = this.chart_obj.colors.lighten(facet.color, template.background_shade);
        }

        var pattern_obj = {
          el: null,
          type: facet.pattern_type,
          id: facet.paint_ref,
          path: template.path,
          secondary_color: facet.secondary_color,
          pattern_color: facet.color,
          width: template.width,
          height: template.height,
          stroke_width: template.stroke_width || 3
        };
        this.insert_pattern(pattern_obj);
        this.active_patterns[pattern_obj.type] = pattern_obj;
      } else {}
    }
  }, {
    key: "insert_pattern",
    value: function insert_pattern(pattern) {
      pattern.el = document.createElementNS(svgns, "pattern");
      pattern.el.setAttribute("id", pattern.id);
      pattern.el.setAttribute("width", pattern.width);
      pattern.el.setAttribute("height", pattern.height);
      pattern.el.setAttribute("patternUnits", "userSpaceOnUse");
      var background_el = document.createElementNS(svgns, "rect");
      background_el.setAttribute("width", pattern.width);
      background_el.setAttribute("height", pattern.height);
      background_el.setAttribute("fill", pattern.secondary_color);
      pattern.el.appendChild(background_el);
      var shape_el = document.createElementNS(svgns, "path");
      shape_el.setAttribute("d", pattern.path);
      shape_el.setAttribute("stroke", pattern.pattern_color);
      shape_el.setAttribute("stroke-width", pattern.stroke_width);
      shape_el.setAttribute("stroke-linecap", "square");
      pattern.el.appendChild(shape_el);
      this.chart_obj.update_defs(pattern.el);
    }
  }, {
    key: "remove_pattern",
    value: function remove_pattern(pattern_id) {
      var pattern = this.patterns[pattern_id];

      if (pattern) {
        pattern.facet_id = null;
        pattern.el.remove();
      }
    }
  }]);
  return PatternLibrary;
}();

var GradientLibrary =
/*#__PURE__*/
function () {
  function GradientLibrary(chart_obj, lightness) {
    (0, _classCallCheck2.default)(this, GradientLibrary);
    this.chart_obj = chart_obj;
    this.lightness = lightness || 4;
    this.active_gradients = {};
  }

  (0, _createClass2.default)(GradientLibrary, [{
    key: "assign_gradient",
    value: function assign_gradient(facet) {
      if (facet.gradient_type) {
        facet.paint_ref = "".concat(facet.gradient_type.replace(/\W/g, "_"), "-").concat(facet.id);

        if (!facet.secondary_color) {
          facet.secondary_color = this.chart_obj.colors.lighten(facet.color, this.lightness);
        }

        var gradient_obj = {
          el: null,
          type: facet.gradient_type,
          id: facet.paint_ref,
          base_color: facet.color,
          secondary_color: facet.secondary_color
        };
        this.insert_gradient(gradient_obj);
        this.active_gradients[gradient_obj.id] = gradient_obj;
      } else {}
    }
  }, {
    key: "insert_gradient",
    value: function insert_gradient(gradient) {
      gradient.el = document.createElementNS(svgns, "linearGradient");
      gradient.el.setAttribute("id", gradient.id);
      gradient.el.setAttribute("x1", "0%");
      gradient.el.setAttribute("x2", "0%");
      gradient.el.setAttribute("y1", "100%");
      gradient.el.setAttribute("y2", "0%");
      gradient.el.setAttribute("spreadMethod", "pad");
      var stop_1_el = document.createElementNS(svgns, "stop");
      stop_1_el.setAttribute("offset", 0);
      stop_1_el.setAttribute("stop-color", gradient.secondary_color);
      gradient.el.appendChild(stop_1_el);
      var stop_2_el = document.createElementNS(svgns, "stop");
      stop_2_el.setAttribute("offset", 1);
      stop_2_el.setAttribute("stop-color", gradient.base_color);
      gradient.el.appendChild(stop_2_el);
      this.chart_obj.update_defs(gradient.el);
    }
  }, {
    key: "remove_gradient",
    value: function remove_gradient(gradient_id) {
      var gradient = this.gradients[gradient_id];

      if (gradient) {
        gradient.facet_id = null;
        gradient.el.remove();
      }
    }
  }]);
  return GradientLibrary;
}();

var Dictionary =
/*#__PURE__*/
function () {
  function Dictionary() {
    (0, _classCallCheck2.default)(this, Dictionary);
    this.keys = [];
    this.vocabs = [];
    this.vocab_keys = [];
    this.vocab_urls = [];
    this.libraries = {
      'state': 'modules/states_vocab.json',
      'month': 'modules/months_vocab.json',
      'country': 'modules/countries_vocab.json'
    };
  }

  (0, _createClass2.default)(Dictionary, [{
    key: "find_vocabs",
    value: function find_vocabs(key) {
      var file_name = this.libraries[key];

      if (file_name) {
        this.vocab_keys.push(key);
        this.vocab_urls.push(file_name);
      }
    }
  }, {
    key: "load_vocabs",
    value: function load_vocabs(callback) {
      var _this8 = this;

      var requests = this.vocab_urls.map(function (url) {
        return fetch("".concat(fizz_base_url, "/").concat(url));
      });
      return Promise.all(requests).then(function (responses) {
        return Promise.all(responses.map(function (r) {
          return r.json();
        }));
      }).then(function (vocabs) {
        vocabs.forEach(function (vocab, index) {
          _this8.vocabs[_this8.vocab_keys[index]] = vocab;
        });
        return true;
      }).catch(function (error) {
        return false;
      });
    }
  }, {
    key: "map_key_to_vocab",
    value: function map_key_to_vocab(vocab, value) {
      var terms_arr = this.vocabs[vocab];

      if (terms_arr) {
        for (var t = 0, t_len = terms_arr.length; t_len > t; ++t) {
          var terms = terms_arr[t];
          var found_obj = null;
          var alternatives_arr = [];

          for (var key in terms) {
            if (terms.hasOwnProperty(key)) {
              var key_value = terms[key];

              if (key_value === value) {
                found_obj = {
                  default_key: key
                };
              } else {
                alternatives_arr.push(key);
              }
            }
          }

          if (found_obj) {
            found_obj.alternative_keys = alternatives_arr;
            return found_obj;
          }
        }

        return null;
      }

      return null;
    }
  }, {
    key: "create_vocab_lookup_array",
    value: function create_vocab_lookup_array(vocab, default_key) {
      var terms_arr = this.vocabs[vocab];

      if (terms_arr) {
        var lookup_obj = {};

        for (var t = 0, t_len = terms_arr.length; t_len > t; ++t) {
          var terms = terms_arr[t];
          var default_key_text = terms[default_key].toLowerCase();
          lookup_obj[default_key_text] = terms;
        }

        return lookup_obj;
      }

      return null;
    }
  }, {
    key: "match_key",
    value: function match_key(key) {}
  }, {
    key: "get_vocab_value_by_key",
    value: function get_vocab_value_by_key(vocab, key, value) {
      var data = this.vocabs[vocab];
      return data.filter(function (data) {
        return data[key] == key;
      });
    }
  }]);
  return Dictionary;
}();

var Facet = function Facet(key, chart_type) {
  (0, _classCallCheck2.default)(this, Facet);
  this.key = key;
  this.id = key.replace(/\W+/g, "");
  this.label = key.splitWords();
  this.datatype = null;
  this.datapoints = [];
  this.categories = [];
  this.label_map = {
    default_key: null,
    label_key: null
  };
  this.selected = false;
  this.chart_type = chart_type || "bar";
  this.axis = null;
  this.order = 0;
  this.bin_details = null;
  this.max_chars = 0;
  this.color = null;
  this.symbol_id = null;
};

var DataRecord = function DataRecord(index) {
  (0, _classCallCheck2.default)(this, DataRecord);
  this.id = utils.generate_unique_id("datarecord_".concat(index));
  this.datapoints = {};
};

var DataPoint = function DataPoint(key, index, value, record_id) {
  (0, _classCallCheck2.default)(this, DataPoint);
  var datatype = datatools.get_datatype(value);
  this.id = utils.generate_unique_id("".concat(key, "_datapoint_").concat(index));
  this.el = null;
  this.record_id = record_id;
  this.key = key;
  this.value = {
    raw: value,
    norm: datatype.norm_value,
    format: null,
    abbr: null,
    display: null
  };
  this.datatype = datatype.datatype;
  this.group = null;
  this.axis_positions = {
    x: {
      x: null,
      y: null
    },
    y: {
      x: null,
      y: null
    },
    r: {
      x: null,
      y: null
    }
  };
  this.popup = {
    el: null,
    x: 0,
    y: 0
  };
};

var Axis =
/*#__PURE__*/
function () {
  function Axis(chart_obj, facets, datatype, has_tick, has_label) {
    (0, _classCallCheck2.default)(this, Axis);
    this.chart_obj = chart_obj;
    this.options = this.chart_obj.options.axis;
    this.facets = facets;
    this.facet_map = new Map();
    this.datatype = this.facets[0].datatype;
    this.axis_type = this.facets[0].axis;
    this.is_mixed_type = false;
    this.is_stacking = false;
    this.tick_position = null;
    this.title = null;
    this.has_tick = has_tick;
    this.has_label = has_label;
    this.is_render = true;
  }

  (0, _createClass2.default)(Axis, [{
    key: "init",
    value: function init() {
      var _this9 = this;

      var label_str = utils.compose_title_from_facets(this.facets);
      this.title = this.options[this.orientation].title.text;

      if (!this.title) {
        this.title = label_str;
      }

      var is_meta = false;

      if ("category" === this.datatype || "location" === this.datatype || "date" === this.datatype) {
        is_meta = true;
      }

      this.label = new Label(this.title, label_str, this.datatype, "axis_label", "heading", this.orientation, is_meta);
      this.el = null;
      this.axis_line = null;
      this.axis_line_id = utils.generate_unique_id("".concat(this.orientation, "_").concat(this.label.title, "_line"));
      this.ortho_axis_line_id = null;
      this.min = null;
      this.max = null;
      this.position = {
        x: null,
        y: null
      };
      this.chart_title_height = +this.chart_obj.options.chart.title.font_size + +this.chart_obj.options.chart.title.margin + this.chart_obj.options.chart.padding;
      this.tick_length = {
        x: this.options.x ? this.options.x.tick.length : null,
        y: this.options.y ? this.options.y.tick.length : null,
        r: this.options.r ? this.options.r.tick.length : null
      };
      this.tick_margin = {
        x: this.options.x ? this.options.x.tick.margin : null,
        y: this.options.y ? this.options.y.tick.margin : null,
        r: this.options.r ? this.options.r.tick.margin : null
      };
      this.tick_font_size = {
        x: this.options.x ? this.options.x.tick.font_size : null,
        y: this.options.y ? this.options.y.tick.font_size : null,
        r: this.options.r ? this.options.r.tick.font_size : null
      };
      this.title_margin = {
        x: this.options.x ? this.options.x.title.margin : null,
        y: this.options.y ? this.options.y.title.margin : null,
        r: this.options.r ? this.options.r.title.margin : null
      };
      this.title_font_size = {
        x: this.options.x ? this.options.x.title.font_size : null,
        y: this.options.y ? this.options.y.title.font_size : null,
        r: this.options.r ? this.options.r.title.font_size : null
      };
      this.datapoint_margin = this.options.datapoint_margin;
      this.label_ids = [];
      this.facets.forEach(function (facet) {
        if (!_this9.facet_map.has(facet.chart_type)) {
          _this9.facet_map.set(facet.chart_type, [facet]);
        } else {
          _this9.facet_map.get(facet.chart_type).push(facet);
        }
      });
      this.get_axis_bounds();
    }
  }, {
    key: "get_axis_bounds",
    value: function get_axis_bounds() {
      if ("single" === this.chart_obj.multiseries || 1 === this.facets.length) {
        this.bounds = this.facets[0].bounds;
      } else if ("stacked" === this.chart_obj.multiseries) {
        if ("numeric" === this.datatype) {
          var all_values = this.facets.reduce(function (arr, facet) {
            return arr.concat(datatools.get_norm_values(facet));
          }, []);
          this.bounds = utils.get_numeric_bounds(all_values, this.orientation, this.options);
          var facet = this.facets[0];
          var sum_values = datatools.get_norm_values(facet);

          for (var f = 1, f_len = this.facets.length; f_len > f; ++f) {
            var each_facet = this.facets[f];
            var values = datatools.get_norm_values(each_facet);

            for (var t = 0, t_len = sum_values.length; t_len > t; ++t) {
              sum_values[t] = sum_values[t] + values[t];
            }
          }

          this.stack_bounds = utils.get_numeric_bounds(sum_values, this.orientation, this.options);
          this.is_mixed_type = 1 < this.facet_map.size ? true : false;
          var bounds_arr = [];
          var bounds_obj = {};
          var stack_bounds_arr = [];
          var stack_bounds_obj = {};
          var _iteratorNormalCompletion2 = true;
          var _didIteratorError2 = false;
          var _iteratorError2 = undefined;

          try {
            for (var _iterator2 = this.facet_map.entries()[Symbol.iterator](), _step2; !(_iteratorNormalCompletion2 = (_step2 = _iterator2.next()).done); _iteratorNormalCompletion2 = true) {
              var _step2$value = (0, _slicedToArray2.default)(_step2.value, 2),
                  chart_type = _step2$value[0],
                  facet_array = _step2$value[1];

              var _all_values = facet_array.reduce(function (arr, facet) {
                return arr.concat(datatools.get_norm_values(facet));
              }, []);

              var bounds = utils.get_numeric_bounds(_all_values, this.orientation, this.options);
              bounds_arr.push(bounds);
              var _facet = facet_array[0];

              var _sum_values = datatools.get_norm_values(_facet);

              for (var _f = 1, _f_len = facet_array.length; _f_len > _f; ++_f) {
                var _each_facet = facet_array[_f];

                var _values = datatools.get_norm_values(_each_facet);

                for (var _t = 0, _t_len = _sum_values.length; _t_len > _t; ++_t) {
                  _sum_values[_t] = _sum_values[_t] + _values[_t];
                }
              }

              var stack_bounds = utils.get_numeric_bounds(_sum_values, this.orientation, this.options);
              stack_bounds_arr.push(stack_bounds);
            }
          } catch (err) {
            _didIteratorError2 = true;
            _iteratorError2 = err;
          } finally {
            try {
              if (!_iteratorNormalCompletion2 && _iterator2.return != null) {
                _iterator2.return();
              }
            } finally {
              if (_didIteratorError2) {
                throw _iteratorError2;
              }
            }
          }

          this.bounds = utils.get_aggregate_bounds(bounds_arr, this.orientation, this.options);
          this.stack_bounds = utils.get_aggregate_bounds(stack_bounds_arr, this.orientation, this.options);
        }
      } else if ("grouped" === this.chart_obj.multiseries) {} else if ("overlap" === this.chart_obj.multiseries) {
        if ("numeric" === this.datatype) {
          var _all_values2 = this.facets.reduce(function (arr, facet) {
            return arr.concat(datatools.get_norm_values(facet));
          }, []);

          this.bounds = utils.get_numeric_bounds(_all_values2, this.orientation, this.options);
        }
      }

      var facet_keys_by_axis = datatools.group_facet_keys_by_axis(this.chart_obj.model.facets);
      var ortho_key = null;

      if ("independent" === this.axis_type && facet_keys_by_axis.has("dependent")) {
        ortho_key = facet_keys_by_axis.get("dependent")[0];
      } else if ("dependent" === this.axis_type && facet_keys_by_axis.has("independent")) {
        ortho_key = facet_keys_by_axis.get("independent")[0];
      }

      var ortho_facets = ortho_key ? this.chart_obj.model.facets[ortho_key] : null;
      var ortho_max_chars = ortho_facets && ortho_facets.bounds && ortho_facets.bounds.max_chars ? ortho_facets.bounds.max_chars : 1;
      this.ortho_axis_offset = 0;

      if ("x" === this.orientation) {
        this.tick_length.x = "numeric" === this.datatype ? this.tick_length.x : 0;
        this.tick_length.y = "numeric" === ortho_facets.datatype ? this.tick_length.y : 0;
        this.ortho_axis_offset = this.tick_length.y + this.tick_margin.y + ortho_max_chars * (this.tick_font_size.y * 0.6) + this.title_margin.y + this.title_font_size.y * 1.2;
        this.length = this.chart_obj.area.width - (this.chart_obj.options.chart.padding * 2 + this.ortho_axis_offset + this.datapoint_margin);
        this.depth = this.tick_length.x + this.tick_margin.x + this.tick_font_size.x + this.title_margin.x + this.title_font_size.x;
      } else if ("y" === this.orientation) {
        this.tick_length.x = "numeric" === ortho_facets.datatype ? this.tick_length.x : 0;
        this.tick_length.y = "numeric" === this.datatype ? this.tick_length.y : 0;
        this.ortho_axis_offset = this.tick_length.x + this.tick_margin.x + this.tick_font_size.x + this.title_margin.x + this.title_font_size.x;
        this.length = this.chart_obj.area.height - (this.ortho_axis_offset + this.chart_obj.options.chart.padding + this.chart_title_height);
        this.depth = this.tick_length.y + this.tick_margin.y + this.bounds.max_chars * (this.tick_font_size.y * 0.6) + this.title_margin.y + this.title_font_size.x * 1.2;
      }

      this.tick_count = this.facets[0].datapoints.length;
      this.labels = datatools.get_norm_values(this.facets[0]);

      if ("numeric" === this.datatype) {
        this.tick_count = this.bounds.interval;
        this.labels = this.bounds.tick_label_array;
        this.datapoint_margin = 0;
      }

      this.tick_distance = (this.length - this.datapoint_margin * this.tick_count - this.datapoint_margin * 2) / this.tick_count;
    }
  }, {
    key: "add_facet",
    value: function add_facet(facet, chart_type, label_text, color, symbol_id, multiseries) {
      if (!this.facet_map.has(chart_type)) {
        this.facet_map.set(chart_type, [facet]);
      } else {
        this.facet_map.get(chart_type).push(facet);
      }

      this.get_axis_bounds();
    }
  }, {
    key: "draw",
    value: function draw(ortho_axis_line_id, is_stacking) {
      this.ortho_axis_line_id = ortho_axis_line_id;
      this.is_stacking = is_stacking ? is_stacking : false;

      if (this.is_render) {
        this.el = document.createElementNS(svgns, "g");
        this.el.setAttribute("role", "graphics-object");
        this.el.setAttribute("aria-roledescription", "".concat(this.orientation, "-axis"));
        this.el.setAttribute("id", "".concat(this.orientation, "-axis"));
        this.el.setAttribute("data-axistype", "".concat(this.datatype));
        this.el.setAttribute("tabindex", "-1");
        this.axis_line = document.createElementNS(svgns, "path");
        this.axis_line.id = this.axis_line_id;
        this.axis_line.classList.add("".concat(this.orientation, "_axis_line"));
        this.has_tick = "numeric" !== this.datatype ? false : this.has_tick;

        if ("numeric" === this.datatype) {
          this.label.el.setAttribute("aria-valuemin", "0");
          this.label.el.setAttribute("aria-valuemax", this.max);
        }

        this.el.appendChild(this.axis_line);
        this.label.el.setAttribute("style", "font-size:".concat(+this.title_font_size.x, "px"));
        this.el.appendChild(this.label.el);
        this.draw_axis();
        this.el.setAttribute("transform", "translate(".concat(this.position.x, ",").concat(this.position.y, ")"));
        this.chart_obj.el.appendChild(this.el);
      } else {
        this.draw_axis();
      }

      this.draw_charts();
    }
  }, {
    key: "draw_charts",
    value: function draw_charts() {
      var _this10 = this;

      if ("dependent" === this.axis_type) {
        var dependent_facets = null;
        var independent_facets = null;

        if ("x" === this.orientation) {
          dependent_facets = this.chart_obj.axes.y.facets;
          independent_facets = this.facets;
        } else if ("y" === this.orientation) {
          dependent_facets = this.facets;
          independent_facets = this.chart_obj.axes.x.facets;
        } else if ("r" === this.orientation) {
          dependent_facets = this.facets;
          independent_facets = this.chart_obj.axes.independent;
        } else {
          throw new Error("We have an axis problem!");
        }

        var charts = [];
        var _iteratorNormalCompletion3 = true;
        var _didIteratorError3 = false;
        var _iteratorError3 = undefined;

        try {
          for (var _iterator3 = this.facet_map.entries()[Symbol.iterator](), _step3; !(_iteratorNormalCompletion3 = (_step3 = _iterator3.next()).done); _iteratorNormalCompletion3 = true) {
            var _step3$value = (0, _slicedToArray2.default)(_step3.value, 2),
                chart_type = _step3$value[0],
                facet_array = _step3$value[1];

            charts.push({
              chart_type: chart_type,
              facet_array: facet_array
            });
          }
        } catch (err) {
          _didIteratorError3 = true;
          _iteratorError3 = err;
        } finally {
          try {
            if (!_iteratorNormalCompletion3 && _iterator3.return != null) {
              _iterator3.return();
            }
          } finally {
            if (_didIteratorError3) {
              throw _iteratorError3;
            }
          }
        }

        charts.sort(function (a, b) {
          return a.facet_array.reduce(function (prev, current) {
            return prev.order > current.order ? prev : current;
          }).order - b.facet_array.reduce(function (prev, current) {
            return prev.order > current.order ? prev : current;
          }).order;
        });
        charts.map(function (each_chart) {
          var chart_generator = _this10.chart_obj.chart_templates[each_chart.chart_type];

          if (chart_generator) {
            var chart = new chart_generator.class(_this10.chart_obj, "title", "horizontal", _this10.chart_obj.axes, independent_facets, each_chart.facet_array);
            chart.draw();
            _this10.chart_obj.charts[each_chart.chart_type] = chart;
          }
        });
        this.update_chart_dimensions();
      }
    }
  }, {
    key: "update_chart_dimensions",
    value: function update_chart_dimensions(x, y, chart) {
      var _this11 = this;

      this.chart_obj.bbox_exclusions.forEach(function (item) {
        return item.el.style.display = "none";
      });
      var bbox = this.chart_obj.el.getBBox();
      var new_chart_height = Math.round(bbox.height) + +this.chart_obj.options.chart.padding * 2;
      this.chart_obj.bbox_exclusions.forEach(function (item) {
        item.el.style.display = "";

        if (item.trim && item.el && item.el.getBBox) {
          var item_bbox = item.el.getBBox();
          var item_translate = item.el.transform.baseVal.consolidate().matrix;
          var item_box = {
            x: item_translate.e + item_bbox.x,
            y: item_translate.f + item_bbox.y,
            width: item_bbox.width,
            height: item_bbox.height * (1 - item.trim)
          };
          var total_height = bbox.height + Math.max(0, item_box.height - (bbox.y + bbox.height - item_box.y));
          new_chart_height = Math.round(total_height) + +_this11.chart_obj.options.chart.padding * 2;
        }
      });
      this.chart_obj.update_viewbox(null, null, null, new_chart_height);
    }
  }, {
    key: "draw_debug_boxes",
    value: function draw_debug_boxes(x, y, chart) {
      var bb = this.el.getBBox();
      this.bbox = document.createElementNS(svgns, "rect");
      this.bbox.setAttribute("id", "".concat(this.orientation, "-axis_bbox"));
      this.bbox.setAttribute("fill", "none");
      this.bbox.setAttribute("stroke", "green");
      this.bbox.setAttribute("stroke-width", "3");
      this.bbox.setAttribute("x", bb.x);
      this.bbox.setAttribute("y", bb.y);
      this.bbox.setAttribute("width", bb.width);
      this.bbox.setAttribute("height", bb.height);
      this.el.appendChild(this.bbox);
      this.frame = document.createElementNS(svgns, "rect");
      this.frame.setAttribute("id", "".concat(this.orientation, "-axis_frame"));
      this.frame.setAttribute("fill", "none");
      this.frame.setAttribute("stroke", "red");
      this.frame.setAttribute("x", 0);
      this.frame.setAttribute("y", 0);
      this.frame.setAttribute("width", this.length);
      this.frame.setAttribute("height", this.depth);

      if ("y" === this.orientation) {
        this.frame.setAttribute("x", -this.depth);
        this.frame.setAttribute("width", this.depth);
        this.frame.setAttribute("height", this.length);
      }

      this.el.appendChild(this.frame);
      this.area = document.createElementNS(svgns, "rect");
      this.area.setAttribute("id", "fizz_chart_area");
      this.area.setAttribute("fill", "none");
      this.area.setAttribute("stroke", "blue");
      this.area.setAttribute("x", this.chart_obj.options.chart.padding);
      this.area.setAttribute("y", this.chart_obj.options.chart.padding);
      this.area.setAttribute("width", this.chart_obj.area.width - this.chart_obj.options.chart.padding * 2);
      this.area.setAttribute("height", this.chart_obj.area.height - this.chart_obj.options.chart.padding * 2);
      this.chart_obj.el.appendChild(this.area);
    }
  }]);
  return Axis;
}();

var XAxis =
/*#__PURE__*/
function (_Axis) {
  (0, _inherits2.default)(XAxis, _Axis);

  function XAxis(chart_obj, facets, datatype, has_tick, has_label) {
    var _this12;

    (0, _classCallCheck2.default)(this, XAxis);
    _this12 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(XAxis).call(this, chart_obj, facets, datatype, has_tick, has_label));
    _this12.orientation = "x";

    _this12.init();

    return _this12;
  }

  (0, _createClass2.default)(XAxis, [{
    key: "draw",
    value: function draw(ortho_axis_line_id, is_stacking) {
      (0, _get2.default)((0, _getPrototypeOf2.default)(XAxis.prototype), "draw", this).call(this, ortho_axis_line_id, is_stacking);
    }
  }, {
    key: "draw_axis",
    value: function draw_axis(max_height, axis_offset, datapoint_width) {
      var _this13 = this;

      this.position.x = this.ortho_axis_offset + this.chart_obj.options.chart.padding;
      this.position.y = this.chart_obj.area.height - (this.depth + this.chart_obj.options.chart.padding);
      var chart_width = 0;

      if (this.tick_distance < this.options.min_interval) {
        this.tick_distance = this.options.min_interval;
        chart_width = this.chart_obj.options.chart.padding * 2 + this.ortho_axis_offset + (this.tick_distance + this.datapoint_margin) * this.tick_count + this.datapoint_margin;
        this.chart_obj.area.width = chart_width;
        this.chart_obj.update_viewbox(null, null, chart_width, null);
        this.length = Math.round(this.chart_obj.area.width - (this.chart_obj.options.chart.padding * 2 + this.ortho_axis_offset + this.datapoint_margin));
      }

      var label_x = Math.round(this.length / 2);
      var label_y = Math.round(this.tick_length.x + this.tick_margin.x + this.tick_font_size.x + this.title_margin.x + this.title_font_size.x);
      this.label.el.setAttribute("transform", "translate(".concat(label_x, ",").concat(label_y, ")"));
      var axis_line_d = "M".concat(-this.tick_length.y, ",0 H").concat(this.length + this.datapoint_margin);
      this.axis_line.setAttribute("d", axis_line_d);
      var label_lookup = null;
      var label_key = null;
      var current_facet = this.facets[0];
      var label_map = current_facet.label_map;

      if (label_map) {
        label_lookup = label_map.lookup;
        label_key = label_map.label_key;
      }

      var tick_label_length = Math.round(this.max_chars_x * (this.tick_font_size.x * 0.6));
      var is_tick_label_horizontal = true;

      if (tick_label_length > this.tick_distance + this.datapoint_margin) {
        is_tick_label_horizontal = false;
      }

      this.tick_position = {
        x: this.tick_distance / 2 + this.datapoint_margin * 2,
        y: null,
        x_increment: this.tick_distance + this.datapoint_margin
      };

      if ("category" === this.datatype || "location" === this.datatype) {} else if ("date" === this.datatype) {} else if ("numeric" === this.datatype) {
        this.tick_position.x = 0;
        this.tick_count++;
      }

      var _loop = function _loop(t, t_len) {
        var tick_text = _this13.labels[t];
        var label_text = tick_text;

        if (label_lookup) {
          var dep_label_obj = label_lookup[tick_text.toLowerCase()];

          if (dep_label_obj) {
            label_text = dep_label_obj[label_key];
          }
        }

        var label_x = Math.round(_this13.tick_position.x + _this13.tick_position.x_increment * t);
        var label_y = Math.round(_this13.tick_length.x + _this13.tick_margin.x + _this13.tick_font_size.x);

        if ("numeric" !== _this13.datatype) {
          _this13.facets.forEach(function (facet) {
            return facet.datapoints[t].axis_positions.x = label_x;
          });
        }

        var x_axis_tick_group_y_offset = 0;
        var tick_group = document.createElementNS(svgns, "g");
        tick_group.setAttribute("transform", "translate(".concat(label_x, ",").concat(x_axis_tick_group_y_offset, ")"));
        tick_group.classList.add("tick_group_x");
        var additional_label_x = 0;
        var each_tick_label = new Label(label_text, tick_text, _this13.key, "axis_label", "axislabel", "x", false);
        var translate = "translate(".concat(additional_label_x, ",").concat(label_y, ")");

        if (!is_tick_label_horizontal) {
          translate = "translate(".concat(additional_label_x, ",").concat(label_y, ") rotate(-90)");
        }

        each_tick_label.el.setAttribute("transform", translate);
        each_tick_label.el.setAttribute("style", "font-size:".concat(_this13.chart_obj.options.axis.x.tick.font_size, "px"));
        tick_group.appendChild(each_tick_label.el);

        if (_this13.has_tick) {
          var tickmark = document.createElementNS(svgns, "use");
          tickmark.setAttributeNS(xlinkns, "href", "#".concat(_this13.ortho_axis_line_id));
          tickmark.setAttribute("y", -(_this13.position.y - _this13.depth + _this13.tick_length.x));

          if (0 === +tick_text) {
            tickmark.classList.add("tickmark_x_0");
          } else {
            tickmark.classList.add("tickmark_x");
          }

          tick_group.appendChild(tickmark);
          var has_gridmark = false;

          if (has_gridmark) {}
        }

        _this13.el.appendChild(tick_group);

        _this13.label_ids.push(each_tick_label.id);
      };

      for (var t = 0, t_len = this.tick_count; t_len > t; ++t) {
        _loop(t, t_len);
      }
    }
  }]);
  return XAxis;
}(Axis);

var YAxis =
/*#__PURE__*/
function (_Axis2) {
  (0, _inherits2.default)(YAxis, _Axis2);

  function YAxis(chart_obj, facets, datatype, has_tick, has_label) {
    var _this14;

    (0, _classCallCheck2.default)(this, YAxis);
    _this14 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(YAxis).call(this, chart_obj, facets, datatype, has_tick, has_label));
    _this14.orientation = "y";

    _this14.init();

    return _this14;
  }

  (0, _createClass2.default)(YAxis, [{
    key: "draw",
    value: function draw(ortho_axis_line_id, is_stacking) {
      (0, _get2.default)((0, _getPrototypeOf2.default)(YAxis.prototype), "draw", this).call(this, ortho_axis_line_id, is_stacking);
    }
  }, {
    key: "draw_axis",
    value: function draw_axis(max_height, axis_offset, datapoint_width) {
      max_height = max_height || this.length;
      axis_offset = axis_offset || this.ortho_axis_offset;
      datapoint_width = datapoint_width || this.tick_distance;
      this.position.x = this.depth + this.chart_obj.options.chart.padding;
      this.position.y = this.chart_title_height;
      var axis_line_d = "M0,".concat(this.length + this.tick_length.x, " V0");
      this.axis_line.setAttribute("d", axis_line_d);

      if ("numeric" === this.datatype) {
        var font_offset_y = Math.round(this.tick_font_size.y / 3);
        var x_pos = Math.round(-(this.tick_length.y + this.tick_margin.y));
        var tick_offset = 0;
        var bounds = this.bounds;

        if (this.is_stacking && this.stack_bounds) {
          bounds = this.stack_bounds;
        }

        var y_pos = null;
        var tick_interval_y = max_height / bounds.interval;

        for (var y = 0, y_len = bounds.interval; y_len >= y; ++y) {
          y_pos = max_height - tick_interval_y * y;
          var tick_group_y = document.createElementNS(svgns, "g");
          tick_group_y.setAttribute("transform", "translate(".concat(tick_offset, ",").concat(y_pos, ")"));
          tick_group_y.classList.add("tick_group_y");
          var y_tick_label = bounds.tick_label_array[y];
          var each_y_label = new Label(y_tick_label, y_tick_label, this.key, "axis_label_y", "axislabel", "y", false);
          each_y_label.el.setAttribute("transform", "translate(".concat(x_pos, ",").concat(font_offset_y, ")"));
          each_y_label.el.setAttribute("style", "font-size:".concat(+this.tick_font_size.y, "px"));
          tick_group_y.appendChild(each_y_label.el);
          var tickmark_y = document.createElementNS(svgns, "use");
          tickmark_y.setAttributeNS(xlinkns, "href", "#".concat(this.ortho_axis_line_id));

          if (+y_tick_label === 0) {
            tickmark_y.classList.add("tickmark_y_0");
          } else {
            tickmark_y.classList.add("tickmark_y");
          }

          tick_group_y.appendChild(tickmark_y);
          this.el.appendChild(tick_group_y);
        }

        this.label.el.setAttribute("aria-valuemin", "0");
        this.label.el.setAttribute("aria-valuemax", this.max);
        var tick_label_length = this.facets[0].bounds.max_chars * (this.tick_font_size.y * 0.6);
        var label_x = -(this.tick_length.y + this.tick_margin.y + tick_label_length + this.title_margin.y);
        var label_y = this.length / 2;
        var transform = "translate(".concat(label_x, ",").concat(label_y, ") rotate(-90)");
        this.label.el.setAttribute("transform", transform);
        this.label.el.setAttribute("style", "font-size:".concat(+this.title_font_size.y, "px"));
        this.el.appendChild(this.label.el);
      } else {}
    }
  }]);
  return YAxis;
}(Axis);

var RadialAxis =
/*#__PURE__*/
function (_Axis3) {
  (0, _inherits2.default)(RadialAxis, _Axis3);

  function RadialAxis(chart_obj, facets, datatype, has_tick, has_label) {
    var _this15;

    (0, _classCallCheck2.default)(this, RadialAxis);
    _this15 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(RadialAxis).call(this, chart_obj, facets, datatype, has_tick, has_label));
    _this15.orientation = "r";
    _this15.is_render = false;

    _this15.init();

    return _this15;
  }

  (0, _createClass2.default)(RadialAxis, [{
    key: "draw",
    value: function draw(ortho_axis_line_id, is_stacking) {
      (0, _get2.default)((0, _getPrototypeOf2.default)(RadialAxis.prototype), "draw", this).call(this, ortho_axis_line_id, is_stacking);
    }
  }, {
    key: "draw_axis",
    value: function draw_axis(max_height, axis_offset, datapoint_width) {
      var chart_width = this.chart_obj.area.width - this.chart_obj.options.chart.padding * 2;
      var chart_height = this.chart_obj.area.height - this.chart_obj.options.chart.padding * 2;
      var independent_facet = this.chart_obj.axes.independent[0];
      var records = independent_facet.datapoints;

      for (var r = 0, r_len = records.length; r_len > r; ++r) {
        var record = records[r];
        this.chart_obj.colors.register_record(record);
        var color_record = this.chart_obj.colors.records.get(record);
        color_record.id = record.id;
        color_record.base = this.chart_obj.colors.palette[color_record.index];
        record.color = color_record.base;
      }

      this.position.x = chart_width / 2 + this.chart_obj.options.chart.padding;
      this.position.y = chart_height / 2 + this.chart_obj.options.chart.padding;
    }
  }]);
  return RadialAxis;
}(Axis);

var Legend =
/*#__PURE__*/
function () {
  function Legend(chart_obj, facets, datatype, has_tick, has_label) {
    (0, _classCallCheck2.default)(this, Legend);
    this.chart_obj = chart_obj;
    this.options = this.chart_obj.options.axis;
    this.el = null;
    this.facets = facets;
    this.title = null;
    this.init();
  }

  (0, _createClass2.default)(Legend, [{
    key: "init",
    value: function init() {
      var label_str = utils.compose_title_from_facets(this.facets);
      this.title = this.options[this.orientation].title.text;

      if (!this.title) {
        this.title = label_str;
      }
    }
  }, {
    key: "draw",
    value: function draw() {
      this.el = document.createElementNS(svgns, "g");
      this.el.setAttribute("id", "".concat(this.orientation, "-legend"));
      this.el.setAttribute("data-legendtype", "".concat(this.datatype));
      this.el.setAttribute("tabindex", "-1");
      this.axis_line = document.createElementNS(svgns, "path");
      this.axis_line.id = this.axis_line_id;
      this.axis_line.classList.add("".concat(this.orientation, "_axis_line"));

      if ("numeric" === this.datatype) {
        this.label.el.setAttribute("aria-valuemin", "0");
        this.label.el.setAttribute("aria-valuemax", this.max);
      }

      this.el.appendChild(this.axis_line);
      this.label.el.setAttribute("style", "font-size:".concat(+this.title_font_size.x, "px"));
      this.el.appendChild(this.label.el);
      this.draw_axis();
      this.el.setAttribute("transform", "translate(".concat(this.position.x, ",").concat(this.position.y, ")"));
      this.chart_obj.el.appendChild(this.el);
    }
  }]);
  return Legend;
}();

var XYChart =
/*#__PURE__*/
function () {
  function XYChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    (0, _classCallCheck2.default)(this, XYChart);
    this.chart_obj = chart_obj;
    this.model = this.chart_obj.model;
    this.orientation = orientation;
    this.chart_type = "XY";
    this.title = title;
    this.el = null;
    this.label = null;
    this.facets = {
      dependent: dependent_facets,
      independent: independent_facets
    };
    this.axes = {
      x: axes.x,
      y: axes.y
    };
    this.styles = {
      series: {},
      classes: []
    };
    this.is_stacking = false;
    this.is_grouping = false;
    this.max_datapoint_size = this.chart_obj.area.width / 2.5;
    this.is_connector = false;
    this.cluster_array = [];
    this.box_size_debug = null;
    this.init();
  }

  (0, _createClass2.default)(XYChart, [{
    key: "init",
    value: function () {
      var _init = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee6() {
        return _regenerator.default.wrap(function _callee6$(_context6) {
          while (1) {
            switch (_context6.prev = _context6.next) {
              case 0:
              case "end":
                return _context6.stop();
            }
          }
        }, _callee6);
      }));

      function init() {
        return _init.apply(this, arguments);
      }

      return init;
    }()
  }, {
    key: "draw",
    value: function draw() {
      this.el = document.createElementNS(svgns, "g");
      this.el.setAttribute("role", "dataset");
      var pos_x = this.axes.y.depth + this.chart_obj.options.chart.padding;
      var pos_y = this.axes.x.chart_title_height;
      var data_group_transform = "translate(".concat(pos_x, ",").concat(pos_y, ")");
      this.el.setAttribute("transform", data_group_transform);
      this.chart_obj.el.appendChild(this.el);
      var is_stack = "stacked" === this.chart_obj.multiseries ? this.is_stacking : false;
      var datapoint_count = this.axes.x.tick_count;

      if ("numeric" === this.axes.x.datatype) {
        datapoint_count = this.facets.dependent[0].datapoints.length;
      }

      var base_axis_stroke_width = 3;
      var max_height = this.axes.x.position.y - this.axes.y.position.y - base_axis_stroke_width / 2;
      var dep_axis = "dependent" === this.axes.y.axis_type ? this.axes.y : this.axes.x;
      var dep_bounds = dep_axis.bounds;

      if (dep_bounds && (is_stack || dep_axis.is_mixed_type)) {
        dep_bounds = dep_axis.stack_bounds;
      }

      var max_value = dep_bounds.label_max;
      var range_value = dep_bounds.label_max - dep_bounds.label_min;
      var min_value = dep_bounds.label_min;
      var independent = {};

      if ("numeric" === this.axes.x.datatype) {
        var x_bounds = this.axes.x.bounds;
        independent.max = x_bounds.label_max;
        independent.min = x_bounds.label_min;
        independent.range = x_bounds.label_max - x_bounds.label_min;
      }

      var dependent = {};
      dependent.max = max_value;
      dependent.min = min_value;
      dependent.range = range_value;
      var axis_offset = utils.get_transform_offset(this.axes.y.el, this.axes.x.el);
      var bg_pos = {
        x: 0,
        y: 0
      };
      var datapoint_pos = {
        x: 0,
        y: 0
      };

      for (var i = 0, i_len = this.facets.independent.length; i_len > i; ++i) {
        var independent_facet = this.facets.independent[i];

        for (var dp = 0, dp_len = datapoint_count; dp_len > dp; ++dp) {
          var series_id = this.axes.x.label_ids[dp];
          var series_group_id = "group-".concat(series_id);
          var is_append_group = false;
          var series_group = document.getElementById(series_group_id);

          if (!series_group) {
            is_append_group = true;
            series_group = document.createElementNS(svgns, "g");
            series_group.setAttribute("id", series_group_id);
            series_group.setAttribute("role", "datapoint_group");
          }

          var independent_datapoint = independent_facet.datapoints[dp];
          datapoint_pos.x = this.axes.x.tick_position.x + this.axes.x.tick_position.x_increment * dp;
          bg_pos.x = datapoint_pos.x;

          if ("numeric" === this.axes.x.datatype) {
            independent.value = independent_datapoint.value.norm;
            independent.text = independent_datapoint.value.format;
            datapoint_pos.x = (independent.value - independent.min) * (this.axes.x.length / independent.range);
            independent.next = undefined !== independent_facet.datapoints[dp + 1] ? independent_facet.datapoints[dp + 1].value.norm : null;
          }

          var multiseries_offsets = null;

          for (var d = 0, d_len = this.facets.dependent.length; d_len > d; ++d) {
            var dependent_facet = this.facets.dependent[d];
            var current_datapoint = dependent_facet.datapoints[dp];
            dependent.value = current_datapoint.value.norm;
            dependent.text = current_datapoint.value.format;
            dependent.offsets = multiseries_offsets ? multiseries_offsets : null;
            dependent.series = "series ".concat(d, ", set ").concat(dp, ": ").concat(dependent_facet.key);
            var next_datapoint = dependent_facet.datapoints[dp + 1];
            dependent.next = undefined !== next_datapoint ? next_datapoint.value.norm : null;
            var prev_datapoint = dependent_facet.datapoints[dp - 1];
            dependent.prev = undefined !== prev_datapoint ? prev_datapoint.value.norm : null;
            dependent.symbol_id = dependent_facet.symbol_id;
            var z_val = 1;

            if (this.z) {
              z_val = this.z.values[dp];
            }

            if (!dependent.prev) {}

            var datapoint_size = this.axes.x.tick_distance;

            if (!this.is_connector) {
              datapoint_size = Math.min(this.max_datapoint_size, this.axes.x.tick_distance);
            }

            var each_datapoint = this.draw_shape(dependent_facet.id, dependent_facet.label, independent, dependent, z_val, datapoint_size, this.axes.x.options.datapoint_margin, max_height, series_id, this.axes.x.length);
            each_datapoint.el.setAttribute("transform", "translate(".concat(datapoint_pos.x, ",").concat(datapoint_pos.y, ")"));
            each_datapoint.el.classList.add("series_".concat(d));
            each_datapoint.el.classList.add(dependent_facet.id);
            this.styles.series[dependent_facet.id] = {
              color: dependent_facet.color,
              paint_ref: dependent_facet.paint_ref
            };

            if ("numeric" === this.axes.x.datatype && "numeric" === this.axes.y.datatype) {
              var datapoint_obj = {};
              datapoint_obj.datapoint = each_datapoint;
              datapoint_obj.x_val = independent.value;
              datapoint_obj.y_val = dependent.value;
              this.cluster_array.push(datapoint_obj);
              independent.prev = independent.value;
            }

            series_group.appendChild(each_datapoint.el);
            multiseries_offsets = each_datapoint.offsets;
          }

          if (is_append_group) {
            this.el.appendChild(series_group);
          }
        }
      }

      this.chart_obj.add_chart_styles(this.styles);
      this.chart_obj.el.appendChild(this.el);

      if (this.box_size_debug) {
        console.log(this.box_size_debug.getBBox());
      }
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id) {
      var base_id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var group_id = "".concat(base_id, "_group");
      var title_id = "".concat(base_id, "_title");
      var value_id = "".concat(base_id, "_value");
      var datapoint_id = "".concat(base_id, "_datapoint");
      var datapoint = {};
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("id", group_id);
      datapoint.el.setAttribute("role", "datapoint");
      return datapoint;
    }
  }, {
    key: "draw_background",
    value: function draw_background(group_el, x_pos, y_pos, width, height, label_id) {
      var start_x = Math.round(-(width / 2));
      var end_x = Math.round(width / 2);
      var bg_bar = document.createElementNS(svgns, "path");
      bg_bar.classList.add("datapoint_background");
      bg_bar.setAttribute("d", "M".concat(start_x, ",0 V").concat(height, " H").concat(end_x, " V0 Z"));
      bg_bar.setAttribute("transform", "translate(".concat(x_pos, ",").concat(y_pos, ")"));
      group_el.insertBefore(bg_bar, group_el.firstChild);
    }
  }, {
    key: "set_color",
    value: function () {
      var _set_color2 = (0, _asyncToGenerator2.default)(
      /*#__PURE__*/
      _regenerator.default.mark(function _callee7(facets) {
        var _this16 = this;

        return _regenerator.default.wrap(function _callee7$(_context7) {
          while (1) {
            switch (_context7.prev = _context7.next) {
              case 0:
                facets.forEach(function (facet) {
                  _this16.chart_obj.colors.register_key(facet.key);

                  var color_key = _this16.chart_obj.colors.keys.get(facet.key);

                  color_key.id = facet.id;
                  color_key.base = _this16.chart_obj.colors.palette[color_key.index];
                  facet.color = color_key.base;
                });

              case 1:
              case "end":
                return _context7.stop();
            }
          }
        }, _callee7);
      }));

      function set_color(_x3) {
        return _set_color2.apply(this, arguments);
      }

      return set_color;
    }()
  }, {
    key: "set_symbol",
    value: function set_symbol(facets) {
      var _this17 = this;

      facets.forEach(function (facet) {
        if (!facet.symbol_id) {
          var symbol_id = null;

          if (_this17.chart_obj.facet_options && _this17.chart_obj.facet_options[facet.key]) {
            symbol_id = _this17.chart_obj.facet_options[facet.key].symbol_id;
          }

          _this17.chart_obj.symbols.assign_symbol(facet, symbol_id);
        }
      });
    }
  }, {
    key: "set_pattern",
    value: function set_pattern(facets) {
      var _this18 = this;

      facets.forEach(function (facet) {
        if (facet.pattern_type) {
          _this18.chart_obj.patterns.assign_pattern(facet);
        }
      });
    }
  }, {
    key: "set_fill",
    value: function set_fill(facets) {
      var _this19 = this;

      facets.forEach(function (facet) {
        if (facet.pattern_type) {
          _this19.chart_obj.patterns.assign_pattern(facet);
        } else if (facet.gradient_type) {
          _this19.chart_obj.gradients.assign_gradient(facet);
        }
      });
    }
  }, {
    key: "draw_trendline",
    value: function draw_trendline(x_arr, y_arr, method) {
      var total_x = 0;
      var total_y = 0;

      for (var i = 0; i < x_arr.length; i++) {
        total_x += x_arr[i];
        total_y += y_arr[i];
      }

      var mean_x = total_x / x_arr.length;
      var mean_y = total_y / y_arr.length;
      var numerator = 0;
      var denominator = 0;

      for (var _i5 = 0; _i5 < x_arr.length; _i5++) {
        numerator += (x_arr[_i5] - mean_x) * (y_arr[_i5] - mean_y);
        denominator += Math.pow(x_arr[_i5] - mean_x, 2);
      }

      var slope = numerator / denominator;
      var y_intercept = mean_y - mean_x * slope;
      var positive_slope_x = (this.axes.y.bounds.label_max - y_intercept) / slope;
      var negative_slope_x = (this.axes.y.bounds.label_min - y_intercept) / slope;
      var max_x_val = null;
      var min_x_val = null;

      if (positive_slope_x > negative_slope_x) {
        max_x_val = positive_slope_x;
        min_x_val = negative_slope_x;
      } else {
        max_x_val = negative_slope_x;
        min_x_val = positive_slope_x;
      }

      var max_y_val = this.axes.x.bounds.label_max * slope + y_intercept;
      var min_y_val = this.axes.x.bounds.label_min * slope + y_intercept;
      var final_x = null;
      var final_y = null;

      if (max_x_val < this.axes.x.bounds.label_max) {
        final_x = max_x_val;
        final_y = max_x_val * slope + y_intercept;
      } else {
        final_x = this.axes.x.bounds.label_max;
        final_y = max_y_val;
      }

      var first_x = null;
      var first_y = null;

      if (min_x_val > this.axes.x.bounds.label_min) {
        first_x = min_x_val;
        first_y = min_x_val * slope + y_intercept;
      } else {
        first_x = this.axes.x.bounds.label_min;
        first_y = min_y_val;
      }

      var grid_view_proportion_x = this.axes.x.length / (this.axes.x.bounds.label_max - this.axes.x.bounds.label_min);
      var grid_view_proportion_y = this.axes.y.length / (this.axes.y.bounds.label_max - this.axes.y.bounds.label_min);
      var chart_y_int = this.axes.y.length - (y_intercept - this.axes.y.bounds.label_min) * grid_view_proportion_y;
      var chart_final_x = Math.round((final_x - this.axes.x.bounds.label_min) * grid_view_proportion_x);
      var chart_final_y = Math.round(this.axes.y.length - (final_y - this.axes.y.bounds.label_min) * grid_view_proportion_y);
      var chart_first_x = Math.round((first_x - this.axes.x.bounds.label_min) * grid_view_proportion_x);
      var chart_first_y = Math.round(this.axes.y.length - (first_y - this.axes.y.bounds.label_min) * grid_view_proportion_y);
      var trendline = document.createElementNS(svgns, "path");
      trendline.classList.add("trendline");
      var trendline_d = "M".concat(chart_first_x, ",").concat(chart_first_y, " L").concat(chart_final_x, ",").concat(chart_final_y);
      trendline.setAttribute("d", trendline_d);
      this.el.appendChild(trendline);
    }
  }, {
    key: "create_point",
    value: function create_point(x, y, color) {
      var dot_shape = document.createElementNS(svgns, "circle");
      var grid_view_proportion_x = this.axes.x.length / (this.axes.x.bounds.label_max - this.axes.x.bounds.label_min);
      var grid_view_proportion_y = this.axes.y.length / (this.axes.y.bounds.label_max - this.axes.y.bounds.label_min);
      var chart_final_x = (x - this.axes.x.bounds.label_min) * grid_view_proportion_x;
      var chart_final_y = this.axes.y.length - (y - this.axes.y.bounds.label_min) * grid_view_proportion_y;
      dot_shape.setAttribute("cx", chart_final_x);
      dot_shape.setAttribute("cy", chart_final_y);
      dot_shape.setAttribute("r", 10);
      var color_str = "fill: none; stroke: ".concat(color, ";");
      dot_shape.setAttribute("style", color_str);
      this.el.appendChild(dot_shape);
    }
  }]);
  return XYChart;
}();

var BarChart =
/*#__PURE__*/
function (_XYChart) {
  (0, _inherits2.default)(BarChart, _XYChart);

  function BarChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this20;

    (0, _classCallCheck2.default)(this, BarChart);
    _this20 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(BarChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this20.chart_type = "BarChart";

    _this20.set_fill(dependent_facets);

    _this20.is_stacking = true;
    _this20.is_grouping = true;
    return _this20;
  }

  (0, _createClass2.default)(BarChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(BarChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id) {
      var base_id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var group_id = "".concat(base_id, "_group");
      var title_id = "".concat(base_id, "_title");
      var value_id = "".concat(base_id, "_value");
      var datapoint_id = "".concat(base_id, "_datapoint");
      var datapoint = {};
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("id", group_id);
      datapoint.el.setAttribute("role", "datapoint");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("bar");
      datapoint.el.setAttribute("aria-labelledby", "".concat(value_id, " ").concat(label_id));
      var start_x = Math.round(-(datapoint_width / 2));
      var end_x = Math.round(datapoint_width / 2);
      var datapoint_height = (dependent.value - dependent.min) * (max_height / dependent.range);
      var start_y = Math.round(max_height - datapoint_height);
      var end_y = Math.round(max_height - (0 - dependent.min) * (max_height / dependent.range));

      if (dependent.min > 0) {
        end_y = max_height;
      } else if (dependent.max < 0) {
        end_y = 0;
      }

      var bar_shape = document.createElementNS(svgns, "path");
      bar_shape.id = datapoint_id;
      var bar_d = "M".concat(start_x, ",").concat(start_y, " V").concat(end_y, " H").concat(end_x, " V").concat(start_y, " Z");
      bar_shape.setAttribute("d", bar_d);
      datapoint.el.appendChild(bar_shape);
      var offset = 0;

      if ("stacked" === this.chart_obj.multiseries && dependent.offsets) {
        offset = dependent.offsets;
        bar_shape.setAttribute("transform", "translate(0,-".concat(offset, ")"));
      }

      datapoint.offsets = datapoint_height + offset;
      datapoint.popup = new Popup(this.chart_obj, dependent.text, facet_label, independent.text, label_id, value_id, title_id, 0, start_y - offset + 5);
      datapoint.el.appendChild(datapoint.popup.el);
      return datapoint;
    }
  }]);
  return BarChart;
}(XYChart);

var LineChart =
/*#__PURE__*/
function (_XYChart2) {
  (0, _inherits2.default)(LineChart, _XYChart2);

  function LineChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this21;

    (0, _classCallCheck2.default)(this, LineChart);
    _this21 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(LineChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this21.chart_type = "LineChart";

    _this21.set_symbol(dependent_facets);

    _this21.is_connector = true;

    _this21.styles.classes.push("data_line");

    _this21.styles.classes.push("data_symbol");

    return _this21;
  }

  (0, _createClass2.default)(LineChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(LineChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id, x_max_length) {
      var base_id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var group_id = "".concat(base_id, "_group");
      var title_id = "".concat(base_id, "_title");
      var value_id = "".concat(base_id, "_value");
      var datapoint_id = "".concat(base_id, "_datapoint");
      var datapoint = {};
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("id", group_id);
      datapoint.el.setAttribute("role", "datapoint");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("mark");
      datapoint.el.setAttribute("aria-labelledby", "".concat(value_id, " ").concat(label_id));
      var title_el = document.createElementNS(svgns, "title");
      title_el.setAttribute("id", title_id);
      title_el.setAttribute("aria-labelledby", "".concat(label_id, " ").concat(value_id));
      datapoint.el.appendChild(title_el);
      var value_el = document.createElementNS(svgns, "metadata");
      value_el.setAttribute("id", value_id);
      value_el.setAttribute("tabindex", "-1");
      value_el.textContent = "".concat(dependent.text, " ").concat(facet_label, ".");
      title_el.appendChild(value_el);
      var render_space = max_height / dependent.range;
      var datapoint_x_pos = 0;
      var datapoint_height = (dependent.value - dependent.min) * render_space;
      var datapoint_top = max_height - datapoint_height;
      var prev_midpoint_x_pos = datapoint_x_pos;
      var prev_midpoint_top = null;

      if (dependent.prev || 0 === dependent.prev) {
        prev_midpoint_x_pos = datapoint_x_pos - datapoint_width / 2 - datapoint_margin / 2 - 0.1;
        var prev_midpoint_diff = Math.min(dependent.value, dependent.prev) + (Math.max(dependent.value, dependent.prev) - Math.min(dependent.value, dependent.prev)) / 2;
        var prev_midpoint_height = (prev_midpoint_diff - dependent.min) * render_space;
        prev_midpoint_top = max_height - prev_midpoint_height;
      }

      var next_midpoint_x_pos = datapoint_x_pos;
      var next_midpoint_top = null;

      if (dependent.next || 0 === dependent.next) {
        next_midpoint_x_pos = datapoint_x_pos + datapoint_width / 2 + datapoint_margin / 2 + 0.1;
        var next_midpoint_diff = Math.min(dependent.value, dependent.next) + (Math.max(dependent.value, dependent.next) - Math.min(dependent.value, dependent.next)) / 2;
        var next_midpoint_height = (next_midpoint_diff - dependent.min) * render_space;
        next_midpoint_top = max_height - next_midpoint_height;
      }

      if (!dependent.prev && !dependent.next) {}

      var line_d = "M".concat(datapoint_x_pos, ",").concat(datapoint_top);

      if (prev_midpoint_top && next_midpoint_top) {
        line_d = "M".concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_top, " L").concat(datapoint_x_pos, ",").concat(datapoint_top, " L").concat(next_midpoint_x_pos, ",").concat(next_midpoint_top);
      } else if (!prev_midpoint_top && next_midpoint_top) {
        line_d = "M".concat(datapoint_x_pos, ",").concat(datapoint_top, " L").concat(next_midpoint_x_pos, ",").concat(next_midpoint_top);
      } else if (prev_midpoint_top && !next_midpoint_top) {
        line_d = "M".concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_top, " L").concat(datapoint_x_pos, ",").concat(datapoint_top);
      }

      var line_shape = document.createElementNS(svgns, "path");
      line_shape.setAttribute("d", line_d);
      line_shape.classList.add("data_line");
      datapoint.el.appendChild(line_shape);
      var size = z_val * this.chart_obj.options.line.base_symbol_size;
      var symbol_el = document.createElementNS(svgns, "use");
      symbol_el.id = datapoint_id;
      symbol_el.setAttributeNS(xlinkns, "href", "#".concat(dependent.symbol_id));
      symbol_el.setAttribute("y", datapoint_top);
      symbol_el.setAttribute("width", size);
      symbol_el.setAttribute("height", size);
      datapoint.el.appendChild(symbol_el);
      datapoint.popup = new Popup(this.chart_obj, dependent.text, facet_label, independent.text, label_id, value_id, title_id, 0, datapoint_top - 35);
      datapoint.el.appendChild(datapoint.popup.el);
      return datapoint;
    }
  }]);
  return LineChart;
}(XYChart);

var AreaChart =
/*#__PURE__*/
function (_XYChart3) {
  (0, _inherits2.default)(AreaChart, _XYChart3);

  function AreaChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this22;

    (0, _classCallCheck2.default)(this, AreaChart);
    _this22 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(AreaChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this22.chart_type = "AreaChart";

    _this22.set_fill(dependent_facets);

    _this22.is_stacking = true;
    _this22.is_connector = true;

    _this22.styles.classes.push("data_line");

    _this22.styles.classes.push("data_area");

    return _this22;
  }

  (0, _createClass2.default)(AreaChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(AreaChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id, x_max_length) {
      var id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var datapoint = {};
      datapoint.offsets = {
        current: null,
        prev: null,
        next: null
      };
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("mark");
      var title = document.createElementNS(svgns, "title");
      title.setAttribute("aria-labelledby", label_id);
      title.textContent = dependent.text;
      datapoint.el.appendChild(title);
      var render_space = max_height / dependent.range;
      var prev_offsets = null;

      if ("stacked" === this.chart_obj.multiseries && dependent.offsets) {
        prev_offsets = dependent.offsets;
      }

      var datapoint_x_pos = 0;
      var datapoint_height = (dependent.value - dependent.min) * render_space;
      datapoint_height += prev_offsets ? prev_offsets.current : 0;
      datapoint.offsets.current = datapoint_height;
      var datapoint_top = max_height - datapoint_height;
      var datapoint_bottom = max_height - (0 - dependent.min) * render_space;

      if (dependent.min > 0) {
        datapoint_bottom = max_height;
      } else if (dependent.max < 0) {
        datapoint_bottom = 0;
      }

      datapoint_bottom -= prev_offsets ? prev_offsets.current : 0;
      var prev_midpoint_x_pos = datapoint_x_pos;
      var prev_midpoint_top = null;
      var prev_midpoint_bottom = datapoint_bottom;

      if (dependent.prev) {
        prev_midpoint_x_pos = datapoint_x_pos - datapoint_width / 2 - datapoint_margin / 2 - 0.1;
        var prev_midpoint_diff = Math.min(dependent.value, dependent.prev) + (Math.max(dependent.value, dependent.prev) - Math.min(dependent.value, dependent.prev)) / 2;
        var prev_midpoint_height = (prev_midpoint_diff - dependent.min) * render_space;
        prev_midpoint_height += prev_offsets ? prev_offsets.prev : 0;
        datapoint.offsets.prev = prev_midpoint_height;
        prev_midpoint_top = max_height - prev_midpoint_height;
        prev_midpoint_bottom = max_height - (prev_offsets ? prev_offsets.prev : 0);
      }

      var next_midpoint_x_pos = datapoint_x_pos;
      var next_midpoint_top = null;
      var next_midpoint_bottom = datapoint_bottom;

      if (dependent.next) {
        next_midpoint_x_pos = datapoint_x_pos + datapoint_width / 2 + datapoint_margin / 2 + 0.1;
        var next_midpoint_diff = Math.min(dependent.value, dependent.next) + (Math.max(dependent.value, dependent.next) - Math.min(dependent.value, dependent.next)) / 2;
        var next_midpoint_height = (next_midpoint_diff - dependent.min) * render_space;
        next_midpoint_height += prev_offsets ? prev_offsets.next : 0;
        datapoint.offsets.next = next_midpoint_height;
        next_midpoint_top = max_height - next_midpoint_height;
        next_midpoint_bottom = max_height - (prev_offsets ? prev_offsets.next : 0);
      }

      if (!dependent.prev && !dependent.next) {
        prev_midpoint_x_pos = datapoint_x_pos - datapoint_width / 2 - datapoint_margin / 2 - 0.1;
        prev_midpoint_bottom = datapoint_bottom;
        datapoint.offsets.prev = prev_midpoint_bottom;
        next_midpoint_x_pos = datapoint_x_pos + datapoint_width / 2 + datapoint_margin / 2 + 0.1;
        next_midpoint_bottom = datapoint_bottom;
        datapoint.offsets.next = next_midpoint_bottom;
      }

      var line_d = "M".concat(next_midpoint_x_pos, ",").concat(next_midpoint_bottom, " L").concat(datapoint_x_pos, ",").concat(datapoint_bottom, " L").concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_bottom, " V").concat(datapoint_top, " H").concat(next_midpoint_x_pos, " Z");
      var area_d = "".concat(line_d);

      if (prev_midpoint_top && next_midpoint_top) {
        var bottom_line_d = "M".concat(next_midpoint_x_pos, ",").concat(next_midpoint_bottom, " L").concat(datapoint_x_pos, ",").concat(datapoint_bottom, " L").concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_bottom);
        var top_line_d = "M".concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_top, " L").concat(datapoint_x_pos, ",").concat(datapoint_top, " L").concat(next_midpoint_x_pos, ",").concat(next_midpoint_top);
        line_d = "".concat(bottom_line_d, " ").concat(top_line_d);
        area_d = "".concat(bottom_line_d, " ").concat(top_line_d.replace("M", "L"), " Z");
      } else if (!prev_midpoint_top && next_midpoint_top) {
        line_d = "M".concat(next_midpoint_x_pos, ",").concat(next_midpoint_bottom, " L").concat(datapoint_x_pos, ",").concat(datapoint_bottom, " V").concat(datapoint_top, " L").concat(next_midpoint_x_pos, ",").concat(next_midpoint_top);
        area_d = "".concat(line_d, " Z");
      } else if (prev_midpoint_top && !next_midpoint_top) {
        line_d = "M".concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_bottom, " L").concat(datapoint_x_pos, ",").concat(datapoint_bottom, " V").concat(datapoint_top, " L").concat(prev_midpoint_x_pos, ",").concat(prev_midpoint_top);
        area_d = "".concat(line_d, " Z");
      }

      var data_shape = document.createElementNS(svgns, "path");
      data_shape.setAttribute("d", area_d);
      data_shape.classList.add("data_area");
      datapoint.el.appendChild(data_shape);
      var data_line = document.createElementNS(svgns, "path");
      data_line.setAttribute("d", line_d);
      data_line.classList.add("data_line");
      datapoint.el.appendChild(data_line);
      var value_id = "";
      var title_id = "";
      datapoint.popup = new Popup(this.chart_obj, dependent.text, facet_label, independent.text, label_id, value_id, title_id, 0, datapoint_top + 25);
      datapoint.el.appendChild(datapoint.popup.el);
      return datapoint;
    }
  }]);
  return AreaChart;
}(XYChart);

var Histogram =
/*#__PURE__*/
function (_XYChart4) {
  (0, _inherits2.default)(Histogram, _XYChart4);

  function Histogram(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this23;

    (0, _classCallCheck2.default)(this, Histogram);
    _this23 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(Histogram).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this23.chart_type = "Histogram";
    return _this23;
  }

  (0, _createClass2.default)(Histogram, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(Histogram.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id) {
      var id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var datapoint = {};
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("bar");
      var title = document.createElementNS(svgns, "title");
      title.setAttribute("aria-labelledby", label_id);
      title.textContent = dependent.text;
      datapoint.el.appendChild(title);
      var datapoint_height = (dependent.value - dependent.min) * (max_height / dependent.range);
      var datapoint_top = max_height - datapoint_height;
      var datapoint_bottom = max_height - (0 - dependent.min) * (max_height / dependent.range);

      if (dependent.min > 0) {
        datapoint_bottom = max_height;
      } else if (dependent.max < 0) {
        datapoint_bottom = 0;
      }

      var x_pos = datapoint_width / 2;
      var bar_shape = document.createElementNS(svgns, "path");
      bar_shape.id = utils.generate_unique_id("".concat(this.orientation, "_").concat(this.title, "_line"));
      var hist_datapoint_width = datapoint_width + datapoint_margin;
      var bar_d = "M0,".concat(datapoint_top, " V").concat(datapoint_bottom, " H").concat(hist_datapoint_width, " V").concat(datapoint_top, " Z");
      bar_shape.setAttribute("d", bar_d);
      datapoint.el.appendChild(bar_shape);
      return datapoint;
    }
  }]);
  return Histogram;
}(XYChart);

var ScatterPlot =
/*#__PURE__*/
function (_XYChart5) {
  (0, _inherits2.default)(ScatterPlot, _XYChart5);

  function ScatterPlot(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this24;

    (0, _classCallCheck2.default)(this, ScatterPlot);
    _this24 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(ScatterPlot).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this24.chart_type = "ScatterPlot";

    _this24.set_symbol(dependent_facets);

    _this24.styles.classes.push("data_symbol");

    return _this24;
  }

  (0, _createClass2.default)(ScatterPlot, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(ScatterPlot.prototype), "draw", this).call(this);

      if (this.chart_obj.options.scatterplot.trendline.active) {
        (0, _get2.default)((0, _getPrototypeOf2.default)(ScatterPlot.prototype), "draw_trendline", this).call(this, this.axes.x.values, this.axes.y.values);
      }

      if (this.chart_obj.options.scatterplot.trendline.polynomial) {
        this.draw_polynomial_trendline(this.axes.x.values, this.axes.y.values, this.chart_obj.options.scatterplot.trendline.polynomial_degree.value);
      }

      if ("numeric" === this.axes.x.datatype && this.chart_obj.options.scatterplot.clusters.active) {
        var k_val = this.chart_obj.options.scatterplot.clusters.count.value;
        this.cluster_points(this.cluster_array, k_val);
      }
    }
  }, {
    key: "draw_background",
    value: function draw_background(group_el, x_pos, y_pos, width, height, label_id) {}
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, independent, dependent, z_val, datapoint_width, datapoint_margin, max_height, label_id) {
      var id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label_id));
      var datapoint = {};
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("mark");
      var title = document.createElementNS(svgns, "title");
      title.setAttribute("aria-labelledby", label_id);
      title.textContent = "".concat(independent.text, ", ").concat(dependent.text);
      datapoint.el.appendChild(title);
      var datapoint_height = (dependent.value - dependent.min) * (max_height / dependent.range);
      var datapoint_top = max_height - datapoint_height;
      var x_pos = datapoint_width / 2;
      var size = z_val * 10;
      var symbol_el = document.createElementNS(svgns, "use");
      symbol_el.id = utils.generate_unique_id("".concat(this.orientation, "_").concat(this.title, "_scatterplot"));
      symbol_el.setAttributeNS(xlinkns, "href", "#".concat(dependent.symbol_id));
      symbol_el.setAttribute("y", datapoint_top);
      symbol_el.setAttribute("width", size);
      symbol_el.setAttribute("height", size);
      datapoint.el.appendChild(symbol_el);
      var value_id = "";
      var title_id = "";
      datapoint.popup = new Popup(this.chart_obj, dependent.text, facet_label, independent.text, label_id, value_id, title_id, 0, datapoint_top - 35);
      datapoint.el.appendChild(datapoint.popup.el);
      return datapoint;
    }
  }, {
    key: "draw_polynomial_trendline",
    value: function draw_polynomial_trendline(x_arr, y_arr, power) {
      var x_matrix = [];

      for (var i = 0; i <= power; ++i) {
        x_matrix.push([]);
      }

      for (var _i6 = 0; _i6 <= 2 * power; ++_i6) {
        var sum = 0;

        if (_i6 === 0) {
          x_matrix[0][0] = power;
          continue;
        }

        for (var j = 0; j < x_arr.length; ++j) {
          var x_power = Math.pow(x_arr[j], _i6);
          sum += x_power;
        }

        var column = _i6;

        if (_i6 > power) {
          column = power;
        }

        var row = 0;

        if (_i6 > power) {
          row = _i6 % power;

          if (row === 0) {
            row = power;
          }
        }

        var loops = _i6;

        if (_i6 > power) {
          loops = power;
        }

        for (row; row <= loops; ++row) {
          x_matrix[row][column] = sum;
          column--;
        }
      }

      var answer_matrix = [];

      for (var _i7 = 0; _i7 <= power; ++_i7) {
        var _sum = 0;

        for (var _j = 0; _j < x_arr.length; ++_j) {
          var product = y_arr[_j] * Math.pow(x_arr[_j], _i7);
          _sum += product;
        }

        answer_matrix.push(_sum);
      }

      var gaussian_matrix = JSON.parse(JSON.stringify(x_matrix));
      gaussian_matrix.push(answer_matrix);

      for (var _i8 = 0; _i8 < power; ++_i8) {
        for (var _j2 = _i8; _j2 < power; ++_j2) {
          if (gaussian_matrix[_i8][_i8] === 0) {
            for (var k = _i8 + 1; k <= power; ++k) {
              if (gaussian_matrix[_i8][k] !== 0) {
                for (var l = 0; l <= power + 1; ++l) {
                  var temp = gaussian_matrix[l][_i8];
                  gaussian_matrix[l][_i8] = gaussian_matrix[l][k];
                  gaussian_matrix[l][k] = temp;
                }

                break;
              }
            }
          }

          var factor = -1 * gaussian_matrix[_i8][_j2 + 1] / gaussian_matrix[_i8][_i8];
          var factor_arr = [];

          for (var _k = 0; _k <= power + 1; ++_k) {
            factor_arr.push(gaussian_matrix[_k][0] * factor);
            gaussian_matrix[_k][_j2 + 1] += gaussian_matrix[_k][_i8] * factor;
          }
        }
      }

      var coefficient_arr = [];

      for (var _i9 = power; _i9 >= 0; --_i9) {
        var _sum2 = 0;
        var co_index = 0;

        for (var _k2 = power; _k2 > _i9; --_k2) {
          _sum2 += gaussian_matrix[_k2][_i9] * coefficient_arr[co_index];
          co_index++;
        }

        var coefficient = (gaussian_matrix[power + 1][_i9] - _sum2) / gaussian_matrix[_i9][_i9];
        coefficient_arr.push(coefficient);
      }

      var num_points = 200;
      var interval = (this.axes.x.bounds.label_max - this.axes.x.bounds.label_min) / num_points;
      var points = [];

      for (var _i10 = 0; _i10 <= num_points; ++_i10) {
        var x_val = Math.round(this.axes.x.bounds.label_min + interval * _i10);
        var y_val = 0;

        for (var _j3 = 0; _j3 < coefficient_arr.length; ++_j3) {
          var _product = Math.round(coefficient_arr[_j3] * Math.pow(x_val, power - _j3));

          y_val += _product;
        }

        var point_val = this.draw_trendline_curve_segments(x_val, y_val);
        points.push(point_val);
      }

      var path_shape = document.createElementNS(svgns, "path");
      path_shape.setAttribute("d", "M".concat(points.join(' ')));
      var color_str = "fill: none; stroke: red;";
      path_shape.setAttribute("style", color_str);
      this.el.appendChild(path_shape);
    }
  }, {
    key: "draw_trendline_curve_segments",
    value: function draw_trendline_curve_segments(x, y) {
      var dot_shape = document.createElementNS(svgns, "circle");
      var grid_view_proportion_x = this.axes.x.length / (this.axes.x.bounds.label_max - this.axes.x.bounds.label_min);
      var grid_view_proportion_y = this.axes.y.length / (this.axes.y.bounds.label_max - this.axes.y.bounds.label_min);
      var chart_final_x = (x - this.axes.x.bounds.label_min) * grid_view_proportion_x;
      var chart_final_y = this.axes.y.length - (y - this.axes.y.bounds.label_min) * grid_view_proportion_y;
      dot_shape.setAttribute("cx", chart_final_x);
      dot_shape.setAttribute("cy", chart_final_y);
      dot_shape.setAttribute("r", 1);
      var color_str = "fill: none; stroke: red;";
      dot_shape.setAttribute("style", color_str);
      return "".concat(chart_final_x, ",").concat(chart_final_y);
    }
  }, {
    key: "cluster_points",
    value: function cluster_points(datapoint_arr, k_val) {
      var final_clusters = [];
      var range_x = this.axes.x.bounds.label_max - this.axes.x.bounds.label_min;
      var range_y = this.axes.y.bounds.label_max - this.axes.y.bounds.label_min;
      var prev_ind_choices = [];
      var centroid_assign = JSON.parse(JSON.stringify(datapoint_arr));
      var chosen_centroids = [];
      var short_dist_arr = [];

      for (var i = 0; i < k_val; i++) {
        var weighted_chances = [];
        var centroid = {};
        var index_choice = null;

        if (i === 0) {
          index_choice = Math.round((centroid_assign.length - 1) * Math.random());
        } else {
          var weights = 0;

          for (var j = 0; j < short_dist_arr.length; j++) {
            weights += Math.pow(short_dist_arr[j], 2);
            weighted_chances.push(weights);
          }

          var rand_weight = Math.round(weights * Math.random());

          for (var _j4 = 0; _j4 < weighted_chances.length; _j4++) {
            if (rand_weight < weighted_chances[_j4]) {
              index_choice = _j4;
              break;
            }
          }
        }

        centroid.x = centroid_assign[index_choice].x_val;
        centroid.y = centroid_assign[index_choice].y_val;
        chosen_centroids.push(centroid);
        centroid_assign.slice(index_choice);
        short_dist_arr = [];

        for (var _j5 = 0; _j5 < centroid_assign.length; _j5++) {
          var current_point = centroid_assign[_j5];
          var dist_arr = [];

          for (var k = 0; k < chosen_centroids.length; k++) {
            var current_centroid = chosen_centroids[k];
            var distance = Math.hypot(current_point.x_val - current_centroid.x, current_point.y_val - current_centroid.y);
            dist_arr.push(distance);
          }

          var short_dist = dist_arr[0];

          for (var _k3 = 0; _k3 < dist_arr.length; _k3++) {
            if (dist_arr[_k3] < short_dist) {
              short_dist = dist_arr[_k3];
            }
          }

          short_dist_arr.push(short_dist);
        }
      }

      var continue_cluster = true;
      var iterate = 0;

      while (continue_cluster) {
        var cluster_groups = [];

        for (var _i11 = 0; _i11 < k_val; _i11++) {
          cluster_groups.push([]);
        }

        var beg_centroid_vals = JSON.parse(JSON.stringify(chosen_centroids));

        for (var _i12 = 0; _i12 < datapoint_arr.length; _i12++) {
          var cur_datapoint = datapoint_arr[_i12];
          var _dist_arr = [];

          for (var _j6 = 0; _j6 < chosen_centroids.length; _j6++) {
            var cur_centroid = chosen_centroids[_j6];

            var _distance = Math.hypot(cur_datapoint.x_val - cur_centroid.x, cur_datapoint.y_val - cur_centroid.y);

            _dist_arr.push(_distance);
          }

          var cluster_assign = 0;
          var least_dist = _dist_arr[0];

          for (var _j7 = 0; _j7 < _dist_arr.length; _j7++) {
            if (_dist_arr[_j7] < least_dist) {
              cluster_assign = _j7;
              least_dist = _dist_arr[_j7];
            }
          }

          cluster_groups[cluster_assign].push(cur_datapoint);
        }

        var color = "pink";

        for (var _i13 = 0; _i13 < cluster_groups.length; _i13++) {
          var total_x = 0;
          var total_y = 0;

          for (var _j8 = 0; _j8 < cluster_groups[_i13].length; _j8++) {
            total_x += cluster_groups[_i13][_j8].x_val;
            total_y += cluster_groups[_i13][_j8].y_val;
          }

          var mean_x = total_x / cluster_groups[_i13].length;
          var mean_y = total_y / cluster_groups[_i13].length;

          if (!isNaN(mean_x)) {
            chosen_centroids[_i13].x = mean_x;
          }

          if (!isNaN(mean_y)) {
            chosen_centroids[_i13].y = mean_y;
          }
        }

        var counter = 0;

        for (var _i14 = 0; _i14 < chosen_centroids.length; _i14++) {
          if (chosen_centroids[_i14].x === beg_centroid_vals[_i14].x && chosen_centroids[_i14].y === beg_centroid_vals[_i14].y) {
            counter++;
          }
        }

        if (counter === chosen_centroids.length) {
          continue_cluster = false;
          final_clusters = cluster_groups;
          cluster_groups[0][0].datapoint.setAttribute("style", "fill: red");
        }
      }

      var point_dis_cent = [];

      for (var _i15 = 0; _i15 < datapoint_arr.length; _i15++) {
        var point_dis_obj = {};
        var _cur_datapoint = datapoint_arr[_i15];
        point_dis_obj.x = _cur_datapoint.x_val;
        point_dis_obj.y = _cur_datapoint.y_val;
        point_dis_obj.cent_dis = [];
        var _dist_arr2 = [];

        for (var _j9 = 0; _j9 < chosen_centroids.length; _j9++) {
          var _cur_centroid = chosen_centroids[_j9];

          var _distance2 = Math.hypot(_cur_datapoint.x_val - _cur_centroid.x, _cur_datapoint.y_val - _cur_centroid.y);

          point_dis_obj.cent_dis.push(_distance2);

          _dist_arr2.push(_distance2);
        }

        point_dis_cent.push(point_dis_obj);
      }

      for (var _i16 = 0; _i16 < final_clusters.length; _i16++) {
        (0, _get2.default)((0, _getPrototypeOf2.default)(ScatterPlot.prototype), "create_point", this).call(this, chosen_centroids[_i16].x, chosen_centroids[_i16].y, "black");

        for (var _j10 = 0; _j10 < final_clusters[_i16].length; _j10++) {
          if (this.chart_obj.colors) {
            var _color = this.chart_obj.colors.palette[_i16];
            var color_str = "fill: ".concat(_color, "; stroke: ").concat(this.chart_obj.colors.accent);

            final_clusters[_i16][_j10].datapoint.setAttribute("style", color_str);
          }
        }
      }
    }
  }]);
  return ScatterPlot;
}(XYChart);

var RadialChart =
/*#__PURE__*/
function () {
  function RadialChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    (0, _classCallCheck2.default)(this, RadialChart);
    this.chart_obj = chart_obj;
    this.model = this.chart_obj.model;
    this.chart_type = dependent_facets[0].chart_type;
    this.options = this.chart_obj.options[this.chart_type] || {};

    if (!this.options.center_label) {
      this.options.center_label = {};
    }

    this.orientation = orientation;
    this.title = title;
    this.el = null;
    this.label = null;
    this.facets = {
      dependent: dependent_facets,
      independent: independent_facets
    };
    this.axes = {
      r: axes.r
    };
    this.radius = this.options.radius || {};
    this.radius = {
      outer: this.radius.outer || "auto",
      inner: this.radius.inner || "auto",
      inner_percent: this.radius.inner_percent || 0.6
    };
    this.styles = {
      series: {},
      classes: []
    };
    this.is_stacking = false;
    this.is_grouping = false;
    this.arc = this.options.arc || 1;
    this.start_angle_offset = this.options.start_angle_offset || 0;
    this.center_label = {
      label: null,
      label_pattern: this.options.center_label.label_pattern,
      is_render: this.options.center_label.is_render || false,
      value: null,
      value_index: this.options.center_label.value_index || 0,
      unit: null,
      subtext: this.options.center_label.subtext || null,
      class: null,
      x: 0,
      y: 0,
      y_offset: this.options.center_label.y_offset || 0.33,
      font_size_percent: this.options.center_label.font_size_percent || 0.8,
      unit_size_percent: this.options.center_label.unit_size_percent || 0.8
    };
    this.arc_type = this.options.arc_type ? this.options.arc_type : "circle";
    this.box_size_debug = null;
    this.label_font_size = chart_obj.options.axis.r.tick.font_size;
    this.label_margin = chart_obj.options.axis.r.tick.margin;
    this.radius_divisor = 2.3;
  }

  (0, _createClass2.default)(RadialChart, [{
    key: "draw",
    value: function draw() {
      this.el = document.createElementNS(svgns, "g");
      this.el.setAttribute("role", "graphics-object");
      this.el.setAttribute("aria-roledescription", "dataset");
      var radial_label_properties = (0, _defineProperty2.default)({
        "fill": "hsl(0, 0%, 0%)",
        "stroke": "none",
        "text-anchor": "middle",
        "font-size": "".concat(this.chart_obj.options.axis.r.tick.font_size, "px")
      }, "fill", this.chart_obj.options.axis.r.tick.font_color ? this.chart_obj.options.axis.r.tick.font_color : "black");
      this.chart_obj.add_style_rule(".radial_label", radial_label_properties, true);
      var chart_width = this.chart_obj.area.width - this.chart_obj.options.chart.padding * 2;
      var chart_height = this.chart_obj.area.height - this.chart_obj.options.chart.padding * 2 - this.axes.r.chart_title_height;

      if ("semicircle" === this.arc_type) {
        this.arc = 0.5;
        this.start_angle_offset = -0.25;
        this.center_label.y_offset = this.options.center_label.y_offset ? this.options.center_label.y_offset : 0;
      }

      var cx = this.chart_obj.area.width / 2;
      var cy = this.axes.r.chart_title_height + chart_height / 2;

      if (!this.radius.outer || "auto" === this.radius.outer) {
        var outer_radius_calc = Math.min(chart_height, chart_width) / this.radius_divisor;

        if (1 > this.arc) {
          outer_radius_calc = Math.max(chart_height, chart_width) / this.radius_divisor;
          this.radius.outer = outer_radius_calc;
          cy = this.axes.r.chart_title_height + this.radius.outer;
        } else {
          this.radius.outer = outer_radius_calc;
        }
      }

      if (1 > this.arc) {
        cy = this.axes.r.chart_title_height + this.radius.outer;
      }

      if (!this.radius.inner || "auto" === this.radius.inner) {
        this.radius.inner = this.radius.outer * this.radius.inner_percent;
      }

      var label_lookup = null;
      var label_key = null;
      var total = this.axes.r.bounds.total;
      var slice_angle = 0;
      var dependent = {};
      var independent = {};
      var datapoint_count = this.axes.r.tick_count;

      if ("numeric" === this.axes.r.datatype) {
        datapoint_count = this.facets.dependent[0].datapoints.length;
      }

      for (var i = 0, i_len = this.facets.independent.length; i_len > i; ++i) {
        var independent_facet = this.facets.independent[i];

        for (var dp = 0, dp_len = datapoint_count; dp_len > dp; ++dp) {
          var series_id = this.axes.r.label_ids[dp];
          var series_group_id = "group-".concat(series_id);
          var independent_datapoint = independent_facet.datapoints[dp];

          for (var d = 0, d_len = this.facets.dependent.length; d_len > d; ++d) {
            var dependent_facet = this.facets.dependent[d];
            var current_datapoint = dependent_facet.datapoints[dp];
            dependent.value = current_datapoint.value.norm;
            dependent.text = current_datapoint.value.format;
            var percentage = dependent.value / total;

            if (this.center_label.is_render && this.center_label.value_index === dp) {
              this.center_label.value = Math.round(percentage * 100);
              this.center_label.unit = "%";
              this.center_label.class = "series_".concat(dp);
            }

            percentage *= this.arc;
            var each_datapoint_label = new Label(independent_datapoint.value.format, independent_datapoint.value.norm, this.axes.r.key, "radial_label", "axislabel", "x", false);
            var each_datapoint = this.draw_shape(dependent_facet.id, dependent_facet.label, dependent, independent, slice_angle, percentage, cx, cy, this.radius, each_datapoint_label, dp, dp_len);
            each_datapoint.el.classList.add("series_".concat(dp));
            each_datapoint.el.classList.add(dependent_facet.id);
            this.styles.series["series_".concat(dp)] = {
              color: independent_datapoint.color,
              paint_ref: dependent_facet.paint_ref
            };
            slice_angle += percentage;
            this.el.appendChild(each_datapoint.el);
          }
        }
      }

      this.draw_center_label(cx, cy);
      this.chart_obj.add_chart_styles(this.styles);
      this.chart_obj.el.appendChild(this.el);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, dependent, independent, slice_angle, percentage, cx, cy, radius, label, index, datapoint_count) {
      var datapoint = this.create_datapoint(facet_id, dependent, independent, label);
      return datapoint;
    }
  }, {
    key: "create_datapoint",
    value: function create_datapoint(facet_id, dependent, independent, label, is_label_display, is_label_meta) {
      var base_id = utils.generate_unique_id("".concat(this.chart_type, "_").concat(facet_id, "_").concat(label.id));
      var datapoint = {
        group_id: "".concat(base_id, "_group"),
        title_id: "".concat(base_id, "_title"),
        value_id: "".concat(base_id, "_value"),
        datapoint_id: "".concat(base_id, "_datapoint")
      };
      datapoint.el = document.createElementNS(svgns, "g");
      datapoint.el.setAttribute("id", datapoint.group_id);
      datapoint.el.setAttribute("role", "graphics-symbol");
      datapoint.el.setAttribute("aria-roledescription", "datapoint");
      datapoint.el.setAttribute("tabindex", "0");
      datapoint.el.classList.add("datapoint");
      datapoint.el.classList.add("slice");
      datapoint.el.setAttribute("aria-labelledby", "".concat(datapoint.value_id, " ").concat(label.id));
      return datapoint;
    }
  }, {
    key: "draw_center_label",
    value: function draw_center_label(cx, cy) {
      if (this.center_label.is_render) {
        this.center_label.x = cx;
        this.center_label.y = cy;
        this.center_label.label = new Label(this.center_label.value, null, null, "center_label", null, null, false);
        this.center_label.label.el.setAttribute("transform", "translate(".concat(cx, ",").concat(cy, ")"));
        this.center_label.label.el.setAttribute("dy", "".concat(this.center_label.y_offset, "em"));
        this.center_label.label.el.setAttribute("tabindex", "0");

        if (this.center_label.unit) {
          var unit_el = document.createElementNS(svgns, "tspan");
          unit_el.textContent = this.center_label.unit;
          unit_el.classList.add("unit");
          this.center_label.label.el.appendChild(unit_el);
        }

        if (this.center_label.subtext) {
          var subtext_el = document.createElementNS(svgns, "tspan");
          subtext_el.textContent = this.center_label.subtext;
          subtext_el.setAttribute("x", "0");
          subtext_el.setAttribute("dy", "2rem");
          subtext_el.classList.add("subtext");
          this.center_label.label.el.appendChild(subtext_el);
        }

        this.center_label.label.el.classList.add(this.center_label.class);
        this.el.appendChild(this.center_label.label.el);
        var center_label_properties = {
          "text-anchor": "middle",
          "font-size": "".concat(this.radius.inner * this.center_label.font_size_percent, "px")
        };
        this.chart_obj.add_style_rule(".center_label", center_label_properties, true);
        var center_label_unit_properties = {
          "font-size": "".concat(this.center_label.unit_size_percent, "em")
        };
        this.chart_obj.add_style_rule(".center_label > tspan.unit", center_label_unit_properties, true);
        this.chart_obj.bbox_exclusions.push({
          el: this.center_label.label.el,
          trim: 0.15
        });
      }
    }
  }]);
  return RadialChart;
}();

var PieChart =
/*#__PURE__*/
function (_RadialChart) {
  (0, _inherits2.default)(PieChart, _RadialChart);

  function PieChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this25;

    (0, _classCallCheck2.default)(this, PieChart);
    _this25 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(PieChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
    _this25.chart_type = "Pie";
    return _this25;
  }

  (0, _createClass2.default)(PieChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(PieChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, dependent, independent, slice_angle, percentage, cx, cy, radius, label, index, datapoint_count) {
      var datapoint = this.create_datapoint(facet_id, dependent, independent, label);
      var p = Math.PI * 2;
      var r = radius.outer;
      var rad = Math.round(r);
      var start_angle = slice_angle += this.start_angle_offset;
      var start_x = Math.round(cx - -r * Math.sin(start_angle * p));
      var start_y = Math.round(cy + -r * Math.cos(start_angle * p));
      var end_x = Math.round(cx - -r * Math.sin((start_angle + percentage) * p));
      var end_y = Math.round(cy + -r * Math.cos((start_angle + percentage) * p));
      var arc_sweep = '0 0 1';

      if (0.5 <= percentage) {
        arc_sweep = '1 1 1';
      }

      if (start_x === end_x && start_y === end_y) {
        end_x += 0.01;
      }

      var path_data = "M".concat(cx, ",").concat(cy, " L").concat(start_x, ",").concat(start_y, " A").concat(rad, ",").concat(rad, " ").concat(arc_sweep, " ").concat(end_x, ",").concat(end_y, " Z");
      var data_shape = document.createElementNS(svgns, "path");
      data_shape.id = datapoint.datapoint_id;
      data_shape.setAttribute("d", path_data);
      datapoint.el.appendChild(data_shape);
      var center_angle = start_angle + percentage / 2;
      var f = this.chart_obj.options.pie.label.margin;
      var radial_label = "radial_label";

      if (0.05 <= center_angle && 0.45 >= center_angle) {
        radial_label = "radial_label_right";
      } else if (0.55 <= center_angle && 0.95 >= center_angle) {
        radial_label = "radial_label_left";
      }

      var font_size = this.chart_obj.options.pie.label.font_size;

      if (0.50 < center_angle && 0.75 >= center_angle) {
        var proportion = (0.75 - center_angle) / 0.25 * font_size;
        f += proportion;
      } else if (0.25 <= center_angle && 0.50 >= center_angle) {
        var _proportion = (0.25 - center_angle) / -0.25 * font_size;

        f += _proportion;
      }

      var label_x = cx - (-r - f) * Math.sin(center_angle * p);
      var label_y = cy + (-r - f) * Math.cos(center_angle * p);
      label.el.setAttribute("transform", "translate(".concat(label_x, ",").concat(label_y, ")"));
      label.el.classList.add(radial_label);
      datapoint.el.appendChild(label.el);
      var label_id = "";
      var value_id = "";
      var title_id = "";

      if (percentage) {
        var popup_x = cx - (-r + 25) * Math.sin(center_angle * p);
        var popup_y = cy + (-r + 25) * Math.cos(center_angle * p);
        datapoint.popup = new Popup(this.chart_obj, dependent.text, label.text, null, label_id, value_id, title_id, popup_x, popup_y - 11);
        datapoint.el.appendChild(datapoint.popup.el);
      }

      return datapoint;
    }
  }]);
  return PieChart;
}(RadialChart);

var DonutChart =
/*#__PURE__*/
function (_RadialChart2) {
  (0, _inherits2.default)(DonutChart, _RadialChart2);

  function DonutChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this26;

    (0, _classCallCheck2.default)(this, DonutChart);
    _this26 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(DonutChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));

    if (!_this26.options.radius || !_this26.options.radius.inner_percent || "auto" === _this26.options.radius.inner_percent) {
      _this26.radius.inner_percent = 0.6;
    }

    return _this26;
  }

  (0, _createClass2.default)(DonutChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(DonutChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, dependent, independent, slice_angle, percentage, cx, cy, radius, label, index, datapoint_count) {
      var datapoint = this.create_datapoint(facet_id, dependent, independent, label);
      var p = Math.PI * 2;
      var r = radius.outer;
      var r2 = radius.inner;
      var rad = Math.round(r);
      var rad2 = Math.round(r2);
      var start_angle = slice_angle += this.start_angle_offset;
      var start_x = Math.round(cx - -r * Math.sin(start_angle * p));
      var start_y = Math.round(cy + -r * Math.cos(start_angle * p));
      var start_x_inner = Math.round(cx - -r2 * Math.sin(start_angle * p));
      var start_y_inner = Math.round(cy + -r2 * Math.cos(start_angle * p));
      var end_x = Math.round(cx - -r * Math.sin((start_angle + percentage) * p));
      var end_y = Math.round(cy + -r * Math.cos((start_angle + percentage) * p));
      var end_x_inner = Math.round(cx - -r2 * Math.sin((start_angle + percentage) * p));
      var end_y_inner = Math.round(cy + -r2 * Math.cos((start_angle + percentage) * p));
      var arc_sweep = "0 0 1";

      if (0.5 <= percentage) {
        arc_sweep = "1 1 1";
      }

      var arc_sweep_inner = "1 0 0";

      if (0.5 <= percentage) {
        arc_sweep_inner = "1 1 0";
      }

      if (start_x === end_x && start_y === end_y) {
        end_x += 0.01;
      }

      if (start_x_inner === end_x_inner && start_y_inner === end_y_inner) {
        end_x_inner += 0.01;
      }

      var path_data = "M".concat(start_x, ",").concat(start_y, " A").concat(rad, ",").concat(rad, " ").concat(arc_sweep, " ").concat(end_x, ",").concat(end_y, " L").concat(end_x_inner, ",").concat(end_y_inner, " A").concat(rad2, ",").concat(rad2, " ").concat(arc_sweep_inner, " ").concat(start_x_inner, ",").concat(start_y_inner, " Z");

      if (percentage) {
        var data_shape = document.createElementNS(svgns, "path");
        data_shape.id = utils.generate_unique_id("".concat(this.orientation, "_").concat(this.title, "_line"));
        data_shape.setAttribute("d", path_data);
        datapoint.el.appendChild(data_shape);
      }

      var center_angle = start_angle + percentage / 2;
      var f = 12;
      var label_x = cx - (-r - f) * Math.sin(center_angle * p);
      var label_y = cy + (-r - f) * Math.cos(center_angle * p);

      if (0.45 <= center_angle && 0.55 >= center_angle) {
        label_y += f;
      }

      var radial_label = "radial_label";

      if (0.05 <= center_angle && 0.45 >= center_angle) {
        radial_label = "radial_label_right";
      } else if (0.55 <= center_angle && 0.95 >= center_angle) {
        radial_label = "radial_label_left";
      }

      label.el.setAttribute("transform", "translate(".concat(label_x, ",").concat(label_y, ")"));
      label.el.classList.add(radial_label);
      datapoint.el.appendChild(label.el);
      var label_id = "";
      var value_id = "";
      var title_id = "";

      if (percentage) {
        var popup_x = cx - (-r + 20) * Math.sin(center_angle * p);
        var popup_y = cy + (-r + 20) * Math.cos(center_angle * p);
        datapoint.popup = new Popup(this.chart_obj, dependent.text, label.text, null, label_id, value_id, title_id, popup_x, popup_y - 11);
        datapoint.el.appendChild(datapoint.popup.el);
      }

      return datapoint;
    }
  }]);
  return DonutChart;
}(RadialChart);

var GaugeChart =
/*#__PURE__*/
function (_RadialChart3) {
  (0, _inherits2.default)(GaugeChart, _RadialChart3);

  function GaugeChart(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    var _this27;

    (0, _classCallCheck2.default)(this, GaugeChart);
    _this27 = (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(GaugeChart).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));

    if (!_this27.options.radius || !_this27.options.radius.inner_percent || "auto" === _this27.options.radius.inner_percent) {
      _this27.radius.inner_percent = 0.8;
    }

    _this27.is_render = _this27.options.center_label.is_render ? _this27.options.center_label.is_render : true;
    _this27.arc = _this27.options.arc ? _this27.options.arc : 1;
    _this27.start_angle_offset = _this27.options.start_angle_offset ? _this27.options.start_angle_offset : -0.5;
    _this27.arc_type = _this27.options.arc_type ? _this27.options.arc_type : "semicircle";
    _this27.radius_divisor = 2.01;
    return _this27;
  }

  (0, _createClass2.default)(GaugeChart, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(GaugeChart.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(facet_id, facet_label, dependent, independent, slice_angle, percentage, cx, cy, radius, label, index, datapoint_count) {
      var datapoint = this.create_datapoint(facet_id, dependent, independent, label);
      var p = Math.PI * 2;
      var r = radius.outer;
      var r2 = radius.inner;
      var rad = Math.round(r);
      var rad2 = Math.round(r2);
      var start_angle = slice_angle += this.start_angle_offset;
      var start_x = Math.round(cx - -r * Math.sin(start_angle * p));
      var start_y = Math.round(cy + -r * Math.cos(start_angle * p));
      var start_x_inner = Math.round(cx - -r2 * Math.sin(start_angle * p));
      var start_y_inner = Math.round(cy + -r2 * Math.cos(start_angle * p));
      var end_x = Math.round(cx - -r * Math.sin((start_angle + percentage) * p));
      var end_y = Math.round(cy + -r * Math.cos((start_angle + percentage) * p));
      var end_x_inner = Math.round(cx - -r2 * Math.sin((start_angle + percentage) * p));
      var end_y_inner = Math.round(cy + -r2 * Math.cos((start_angle + percentage) * p));
      var arc_sweep = "0 0 1";

      if (0.5 <= percentage) {
        arc_sweep = "1 1 1";
      }

      var arc_sweep_inner = "1 0 0";

      if (0.5 <= percentage) {
        arc_sweep_inner = "1 1 0";
      }

      if (start_x === end_x && start_y === end_y) {
        end_x += 0.01;
      }

      if (start_x_inner === end_x_inner && start_y_inner === end_y_inner) {
        end_x_inner += 0.01;
      }

      var path_data = "M".concat(start_x, ",").concat(start_y, " A").concat(rad, ",").concat(rad, " ").concat(arc_sweep, " ").concat(end_x, ",").concat(end_y, " L").concat(end_x_inner, ",").concat(end_y_inner, " A").concat(rad2, ",").concat(rad2, " ").concat(arc_sweep_inner, " ").concat(start_x_inner, ",").concat(start_y_inner, " Z");

      if (percentage) {
        var data_shape = document.createElementNS(svgns, "path");
        data_shape.id = utils.generate_unique_id("".concat(this.orientation, "_").concat(this.title, "_line"));
        data_shape.setAttribute("d", path_data);
        datapoint.el.appendChild(data_shape);
      }

      var label_x = cx - r;
      var label_y = cy + r + +this.chart_obj.options.axis.r.tick.margin;

      if ("semicircle" === this.arc_type) {
        label_y = cy + +this.chart_obj.options.axis.r.tick.font_size + +this.chart_obj.options.axis.r.tick.margin;
      }

      var label_align_class = "label_align_start";
      var label_align = "end";

      if (datapoint_count - 1 === index) {
        label_x = cx + r;
        label_align_class = "label_align_end";
        label_align = "start";
      }

      var label_align_properties = {
        'text-anchor': label_align
      };
      this.chart_obj.add_style_rule(".".concat(label_align_class), label_align_properties, true);
      label.el.setAttribute("transform", "translate(".concat(label_x, ",").concat(label_y, ")"));
      label.el.classList.add(label_align_class);
      label.el.textContent = "".concat(dependent.text, " ").concat(label.el.textContent);
      datapoint.el.appendChild(label.el);
      var label_id = "";
      var value_id = "";
      var title_id = "";

      if (percentage) {
        var center_angle = start_angle + percentage / 2;
        var popup_x = cx - (-r + 20) * Math.sin(center_angle * p);
        var popup_y = cy + (-r + 20) * Math.cos(center_angle * p);
        datapoint.popup = new Popup(this.chart_obj, dependent.text, label.text, null, label_id, value_id, title_id, popup_x, popup_y - 11);
        datapoint.el.appendChild(datapoint.popup.el);
      }

      return datapoint;
    }
  }]);
  return GaugeChart;
}(RadialChart);

var Table =
/*#__PURE__*/
function () {
  function Table(chart_obj, title, orientation, keys) {
    (0, _classCallCheck2.default)(this, Table);
    this.chart_obj = chart_obj;
    this.orientation = orientation;
    this.title = title;
    this.keys = keys || this.chart_obj.model.keys;
    this.label = null;
    this.el = null;
  }

  (0, _createClass2.default)(Table, [{
    key: "draw",
    value: function draw() {
      this.el = document.createElement("table");
      var datapoint_count = 0;
      var header_row_el = this.el.insertRow();

      for (var k = 0, k_len = this.keys.length; k_len > k; ++k) {
        var key = this.keys[k];
        var facet = this.chart_obj.model.facets[key];
        var class_list = [];

        if (facet.selected) {
          class_list.push("selected");
        }

        var each_datapoint = this.draw_shape("th", facet.label, class_list);
        header_row_el.appendChild(each_datapoint.el);
        var key_len = this.chart_obj.model.facets[key].datapoints.length;
        datapoint_count = Math.max(datapoint_count, key_len);
      }

      for (var row = 0; datapoint_count > row; ++row) {
        var row_el = this.el.insertRow();

        for (var col = 0, col_len = this.keys.length; col_len > col; ++col) {
          var _key2 = this.keys[col];
          var datapoint = this.chart_obj.model.facets[_key2].datapoints[row];
          var val = datapoint.value.format;
          var datatype = datapoint.datatype;

          var _each_datapoint = this.draw_shape("td", val, [datatype]);

          row_el.appendChild(_each_datapoint.el);
        }
      }

      return this.el;
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(el_type, val, class_list) {
      var datapoint = {};
      datapoint.el = document.createElement(el_type);

      if (class_list) {
        var _datapoint$el$classLi;

        (_datapoint$el$classLi = datapoint.el.classList).add.apply(_datapoint$el$classLi, (0, _toConsumableArray2.default)(class_list));
      }

      datapoint.el.textContent = val;
      return datapoint;
    }
  }]);
  return Table;
}();

var HeatMap =
/*#__PURE__*/
function (_Table) {
  (0, _inherits2.default)(HeatMap, _Table);

  function HeatMap(chart_obj, title, orientation, axes, independent_facets, dependent_facets) {
    (0, _classCallCheck2.default)(this, HeatMap);
    return (0, _possibleConstructorReturn2.default)(this, (0, _getPrototypeOf2.default)(HeatMap).call(this, chart_obj, title, orientation, axes, independent_facets, dependent_facets));
  }

  (0, _createClass2.default)(HeatMap, [{
    key: "draw",
    value: function draw() {
      (0, _get2.default)((0, _getPrototypeOf2.default)(HeatMap.prototype), "draw", this).call(this);
    }
  }, {
    key: "draw_shape",
    value: function draw_shape(dependent, independent, slice_angle, percentage, cx, cy, radius, label) {}
  }]);
  return HeatMap;
}(Table);

var Label =
/*#__PURE__*/
function () {
  function Label(text, title, datatype, class_name, role, axis, is_meta) {
    (0, _classCallCheck2.default)(this, Label);
    this.el = null;
    this.id = null;
    this.text = text;
    this.title = title;
    this.datatype = datatype;
    this.class_name = class_name;
    this.role = role;
    this.axis = axis;
    this.is_meta = is_meta;
    this.init();
  }

  (0, _createClass2.default)(Label, [{
    key: "init",
    value: function init() {
      var id_seed = "".concat(this.axis, "_").concat(this.title);

      if (!this.axis) {
        id_seed = "".concat(this.class_name, "_").concat(this.title);
      }

      this.id = utils.generate_unique_id(id_seed);
      var el_type = "text";

      if (this.is_meta) {
        el_type = "title";
      }

      this.el = document.createElementNS(svgns, el_type);
      this.el.classList.add(this.class_name);
      this.el.textContent = this.text;

      if (this.title && this.text !== this.title) {
        this.el.textContent = this.text + " ";
        var title_el = document.createElementNS(svgns, "title");
        title_el.id = this.id;
        title_el.textContent = this.title;
        this.el.appendChild(title_el);
      } else {
        this.el.id = this.id;
      }

      return this.el;
    }
  }]);
  return Label;
}();

var Popup =
/*#__PURE__*/
function () {
  function Popup(chart_obj, value, label_text, facet_text, label_id, value_id, title_id, x, y) {
    (0, _classCallCheck2.default)(this, Popup);
    this.chart_obj = chart_obj;
    this.el = null;
    this.value = value;
    this.label_text = label_text;
    this.facet_text = facet_text;
    this.label_id = label_id;
    this.value_id = value_id;
    this.title_id = title_id;
    this.x = x;
    this.y = y;
    this.init();
  }

  (0, _createClass2.default)(Popup, [{
    key: "init",
    value: function init() {
      this.el = document.createElementNS(svgns, "g");
      this.el.classList.add("datapoint_popup");
      this.el.setAttribute("transform", "translate(".concat(this.x, ",").concat(this.y, ")"));
      var label_el = document.createElementNS(svgns, "text");
      label_el.setAttribute("id", this.title_id);
      label_el.setAttribute("aria-labelledby", "".concat(this.label_id, " ").concat(this.value_id));
      label_el.setAttribute("x", "0");
      label_el.setAttribute("y", "17");
      var value_el = document.createElementNS(svgns, "tspan");
      value_el.setAttribute("id", this.value_id);
      label_el.setAttribute("x", "0");
      value_el.setAttribute("tabindex", "-1");
      value_el.textContent = this.value;
      label_el.appendChild(value_el);
      var desc_el = document.createElementNS(svgns, "tspan");
      desc_el.setAttribute("id", this.label_id);
      desc_el.classList.add("desc");
      desc_el.setAttribute("x", "0");
      desc_el.setAttribute("dy", "1em");
      desc_el.setAttribute("tabindex", "-1");
      desc_el.textContent = " ".concat(this.label_text);
      var facet_el = null;

      if (this.facet_text) {
        desc_el.textContent = " ".concat(this.label_text, ", ");
        facet_el = document.createElementNS(svgns, "tspan");
        facet_el.classList.add("desc");
        facet_el.setAttribute("x", "0");
        facet_el.setAttribute("dy", "1em");
        facet_el.setAttribute("tabindex", "-1");
        facet_el.textContent = this.facet_text;
      }

      this.chart_obj.root.appendChild(label_el);
      var value_bb = null;

      if (value_el.getBBox) {
        value_bb = value_el.getBBox();
      } else {
        value_bb = {
          x: 0,
          y: 3,
          width: value_el.textContent.length * 5 + 10,
          height: 20
        };
      }

      var value_bg_el = document.createElementNS(svgns, "rect");
      value_bg_el.setAttribute("x", value_bb.x - value_bb.width / 2 - 5);
      value_bg_el.setAttribute("y", value_bb.y - 5);
      value_bg_el.setAttribute("width", value_bb.width + 10);
      value_bg_el.setAttribute("height", value_bb.height + 10);
      value_bg_el.setAttribute("rx", "1");
      value_bg_el.setAttribute("ry", "1");
      this.el.appendChild(value_bg_el);
      label_el.appendChild(desc_el);

      if (facet_el) {
        label_el.appendChild(desc_el);
      }

      var label_bb = null;

      if (label_el.getBBox) {
        label_bb = label_el.getBBox();
      } else {
        label_bb = {
          x: 0,
          y: 3,
          width: label_el.textContent.length * 5 + 10,
          height: 35
        };
      }

      var label_bg_el = document.createElementNS(svgns, "rect");
      label_bg_el.classList.add("desc");
      label_bg_el.setAttribute("x", label_bb.x - label_bb.width / 2 - 5);
      label_bg_el.setAttribute("y", label_bb.y - 5);
      label_bg_el.setAttribute("width", label_bb.width + 10);
      label_bg_el.setAttribute("height", label_bb.height + 10);
      label_bg_el.setAttribute("rx", "1");
      label_bg_el.setAttribute("ry", "1");
      this.el.appendChild(label_bg_el);
      this.el.appendChild(label_el);
      return this.el;
    }
  }]);
  return Popup;
}();

var number_names = ['nones', 'ones', 'tens', 'hundreds', 'thousands', 'ten thousands', 'hundred thousands', 'millions', 'ten millions', 'hundred millions', 'billions', 'ten billions', 'hundred billions', 'trillions', 'ten trillions', 'hundred trillions'];

var Colors =
/*#__PURE__*/
function () {
  function Colors() {
    (0, _classCallCheck2.default)(this, Colors);
    this.palette = null;
    this.primary = 'hsl(270, 50%, 50%)';
    this.accent = 'hsl(270, 50%, 25%)';
    this.active = 'hsl(270, 50%, 65%)';
    this.keys = new Map();
    this.records = new Map();
    this.palettes = {
      'palette-0': {
        'name': 'diva',
        'title': 'diva (color-blind safe)',
        'type': 'category',
        'colors': ['hsl(210, 54%, 38%)', 'hsl(20, 89%, 42%)', 'hsl(75, 43%, 45%)', 'hsl(40, 98%, 69%)', 'hsl(215, 37%, 66%)', 'hsl(63, 100%, 23%)', 'hsl(34, 57%, 46%)', 'hsl(51, 56%, 64%)', 'hsl(253, 26%, 43%)', 'hsl(85, 65%, 36%)', 'hsl(12, 56%, 51%)', 'hsl(30, 42%, 35%)', 'hsl(0, 100%, 50%)', 'hsl(240, 100%, 50%)', 'hsl(120, 100%, 50%)', 'hsl(39, 100%, 50%)', 'hsl(300, 100%, 25%)', 'hsl(51, 100%, 50%)', 'hsl(328, 100%, 54%)', 'hsl(177, 70%, 41%)', 'hsl(348, 83%, 47%)', 'hsl(248, 53%, 58%)', 'hsl(240, 64%, 27%)', 'hsl(160, 51%, 60%)', 'hsl(9, 100%, 64%)', 'hsl(300, 76%, 72%)', 'hsl(60, 100%, 50%)', 'hsl(0, 0%, 50%)', 'hsl(15, 72%, 70%)', 'hsl(43, 89%, 38%)', 'hsl(120, 25%, 65%)', 'hsl(84, 100%, 59%)', 'hsl(300, 100%, 50%)', 'hsl(248, 39%, 39%)', 'hsl(271, 76%, 53%)', 'hsl(0, 68%, 42%)', 'hsl(120, 73%, 75%)', 'hsl(240, 100%, 25%)', 'hsl(28, 87%, 67%)', 'hsl(120, 100%, 25%)', 'hsl(340, 60%, 65%)', 'hsl(180, 25%, 25%)', 'hsl(16, 100%, 66%)', 'hsl(195, 100%, 50%)', 'hsl(0, 25%, 65%)', 'hsl(282, 100%, 41%)', 'hsl(25, 76%, 31%)', 'hsl(80, 60%, 35%)', 'hsl(0, 0%, 0%)', 'hsl(302, 59%, 65%)', 'hsl(0, 59%, 41%)', 'hsl(182, 25%, 50%)', 'hsl(90, 100%, 50%)', 'hsl(25, 75%, 47%)', 'hsl(219, 79%, 66%)', 'hsl(240, 100%, 27%)', 'hsl(180, 100%, 27%)', 'hsl(120, 100%, 20%)', 'hsl(56, 38%, 58%)', 'hsl(300, 100%, 27%)', 'hsl(300, 100%, 50%)', 'hsl(82, 39%, 30%)', 'hsl(33, 100%, 50%)', 'hsl(280, 61%, 50%)', 'hsl(0, 100%, 27%)', 'hsl(0, 0%, 41%)', 'hsl(181, 100%, 41%)', 'hsl(120, 61%, 34%)', 'hsl(43, 74%, 49%)', 'hsl(330, 100%, 71%)', 'hsl(0, 53%, 58%)', 'hsl(210, 100%, 56%)', 'hsl(275, 100%, 25%)', 'hsl(90, 100%, 49%)', 'hsl(0, 79%, 72%)', 'hsl(210, 14%, 53%)', 'hsl(120, 61%, 50%)', 'hsl(0, 100%, 25%)', 'hsl(240, 100%, 40%)', 'hsl(288, 59%, 58%)', 'hsl(147, 50%, 47%)', 'hsl(249, 80%, 67%)', 'hsl(17, 100%, 74%)', 'hsl(178, 60%, 55%)', 'hsl(322, 81%, 43%)', 'hsl(60, 100%, 25%)', 'hsl(16, 100%, 50%)', 'hsl(260, 60%, 65%)', 'hsl(30, 59%, 53%)', 'hsl(300, 47%, 75%)', 'hsl(157, 100%, 49%)', 'hsl(225, 73%, 57%)', 'hsl(6, 93%, 71%)', 'hsl(146, 50%, 36%)', 'hsl(19, 56%, 40%)', 'hsl(210, 13%, 50%)', 'hsl(150, 100%, 50%)', 'hsl(207, 44%, 49%)', 'hsl(34, 44%, 69%)', 'hsl(180, 100%, 25%)']
      },
      'palette-1': {
        'name': 'warm',
        'title': 'warm hues (color-blind safe)',
        'type': 'category',
        'colors': ['hsl(38, 96%, 58%)', 'hsl(82, 77%, 40%)', 'hsl(54, 81%, 73%)', 'hsl(22, 97%, 51%)', 'hsl(77, 98%, 25%)']
      },
      'palette-2': {
        'name': 'cold',
        'title': 'cold hues (color-blind safe)',
        'type': 'category',
        'colors': ['hsl(223, 100%, 70%)', 'hsl(331, 72%, 51%)', 'hsl(23, 100%, 50%)', 'hsl(251, 83%, 65%)', 'hsl(41, 100%, 50%)']
      },
      'palette-3': {
        'name': 'rainbow',
        'title': 'rainbow (color-blind safe)',
        'type': 'category',
        'colors': ['hsl(270, 100%, 29%)', 'hsl(330, 100%, 71%)', 'hsl(30, 100%, 43%)', 'hsl(180, 100%, 14%)', 'hsl(210, 100%, 43%)', 'hsl(0, 100%, 29%)', 'hsl(120, 100%, 57%)', 'hsl(60, 100%, 71%)', 'hsl(330, 100%, 86%)', 'hsl(210, 100%, 86%)', 'hsl(30, 100%, 29%)', 'hsl(180, 100%, 29%)', 'hsl(270, 100%, 71%)', 'hsl(210, 100%, 71%)', 'hsl(0, 0%, 0%)']
      }
    };
    this.set_palette(0);
  }

  (0, _createClass2.default)(Colors, [{
    key: "register_key",
    value: function register_key(key) {
      if (!this.keys.has(key)) {
        this.keys.set(key, {
          index: this.keys.size,
          base: null,
          light: null,
          dark: null
        });
      }
    }
  }, {
    key: "register_record",
    value: function register_record(record) {
      if (!this.records.has(record)) {
        this.records.set(record, {
          index: this.records.size,
          base: null,
          light: null,
          dark: null
        });
      }
    }
  }, {
    key: "set_colors",
    value: function set_colors(color_obj) {
      if (!color_obj.palette) {
        this.set_palette(0);
      } else {
        this.palette = color_obj.palette.concat(palette);
      }

      this.primary = color_obj.primary;
      this.accent = color_obj.accent;
      this.active = color_obj.active;
    }
  }, {
    key: "set_palette",
    value: function set_palette(id) {
      if ("number" !== typeof id) {
        id = 0;
      }

      var palette_data = this.palettes["palette-".concat(id)];

      if (!palette_data) {
        palette_data = this.palettes["palette-0"];
      }

      this.palette = palette_data.colors;
    }
  }, {
    key: "get_palettes",
    value: function get_palettes(palette_ids) {
      if (!palette_ids) {
        return this.palettes;
      }

      if ("string" === typeof palette_ids) {
        return this.palettes[palette_ids];
      }

      var palettes = [];

      for (var i = 0, i_len = palette_ids.length; i_len > i; ++i) {
        var _palette = this.palettes[palette_ids[i]];

        if (!_palette) {
          _palette = this.palettes["palette-".concat(palette_ids[i])];
        }

        if (_palette) {
          palettes.push(_palette);
        }
      }

      return palettes;
    }
  }, {
    key: "set_palette_color_by_index",
    value: function set_palette_color_by_index(palette_id, index, color) {
      var palette = this.palettes[palette_id];

      if (palette) {
        palette[index] = color;
      }
    }
  }, {
    key: "create_palette",
    value: function create_palette(id, colors, metadata) {
      var palette = this.palettes[id];

      if (!palette) {
        this.palettes[id] = {};
        palette = this.palettes[id];
      }

      palette.name = metadata.name;
      palette.title = metadata.title;
      palette.type = metadata.type;
      palette.colors = colors;
    }
  }, {
    key: "get_hsl_components",
    value: function get_hsl_components(hsla) {
      var hsl_regex = /hsl[a]?\(\s*(-?\d+|-?\d*.\d+)\s*,\s*(-?\d+|-?\d*.\d+)%\s*,\s*(-?\d+|-?\d*.\d+)%\s*\)/;
      var hsl_array = hsla.match(hsl_regex);
      var hsla_components = {
        hue: +hsl_array[1],
        h: +hsl_array[1],
        saturation: +hsl_array[2],
        s: +hsl_array[2],
        lightness: +hsl_array[3],
        l: +hsl_array[3],
        alpha: 1,
        a: 1
      };
      return hsla_components;
    }
  }, {
    key: "lighten",
    value: function lighten(hsl, shade_count) {
      var hsl_comp = this.get_hsl_components(hsl);
      var h = hsl_comp.hue;
      var s = hsl_comp.saturation;
      var l = hsl_comp.lightness;
      var new_l = Math.min(l + shade_count * 5, 100);
      return "hsl(".concat(h, ", ").concat(s, "%, ").concat(new_l, "%)");
    }
  }, {
    key: "generate_sequential_palette",
    value: function generate_sequential_palette(hsl, count, is_lighter, palette_id) {
      var hsl_comp = this.get_hsl_components(hsl);
      var h = hsl_comp.hue;
      var s = hsl_comp.saturation;
      var l = hsl_comp.lightness;
      var s_range = s - 15;

      if (is_lighter) {
        s_range = 85 - s;
      }

      var s_interval = Math.round(s_range / count / 5);
      var l_range = l - 15;

      if (is_lighter) {
        l_range = 85 - l;
      }

      var l_interval = Math.round(l_range / count);
      var palette = [];

      for (var i = 0, i_len = count; i_len > i; ++i) {
        palette.push("hsl(".concat(h, ", ").concat(s, "%, ").concat(l, "%)"));

        if (is_lighter) {
          s += s_interval;
          l += l_interval;
        } else {
          s -= s_interval;
          l -= l_interval;
        }
      }

      if (palette_id) {
        this.create_palette(palette_id, palette, {
          name: palette_id,
          title: palette_id,
          type: "numeric"
        });
      } else {
        return palette;
      }
    }
  }, {
    key: "generate_interpolation_palette",
    value: function generate_interpolation_palette(hsl1, hsl2, count, palette_id) {
      var hsl_comp1 = this.get_hsl_components(hsl1);
      var h1 = hsl_comp1.hue;
      var s1 = hsl_comp1.saturation;
      var l1 = hsl_comp1.lightness;
      var hsl_comp2 = this.get_hsl_components(hsl2);
      var h2 = hsl_comp2.hue;
      var s2 = hsl_comp2.saturation;
      var l2 = hsl_comp2.lightness;
      count -= 1;
      var h_range = h1 - h2;
      var h_interval = h_range / count;
      var s_range = s1 - s2;
      var s_interval = s_range / count;
      var l_range = l1 - l2;
      var l_interval = l_range / count;
      var palette = [];

      for (var i = 0, i_len = count; i_len > i; ++i) {
        palette.push("hsl(".concat(Math.round(h1), ", ").concat(Math.round(s1), "%, ").concat(Math.round(l1), "%)"));
        h1 -= h_interval;
        s1 -= s_interval;
        l1 -= l_interval;
      }

      palette.push("hsl(".concat(h2, ", ").concat(s2, "%, ").concat(l2, "% )"));
      this.create_palette(palette_id, palette, {
        name: palette_id,
        title: palette_id,
        type: "numeric"
      });
    }
  }, {
    key: "hsl_to_hex",
    value: function hsl_to_hex(h, s, l) {
      h /= 360;
      s /= 100;
      l /= 100;
      var r, g, b;

      if (s === 0) {
        r = g = b = l;
      } else {
        var hue2rgb = function hue2rgb(p, q, t) {
          if (t < 0) t += 1;
          if (t > 1) t -= 1;
          if (t < 1 / 6) return p + (q - p) * 6 * t;
          if (t < 1 / 2) return q;
          if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
          return p;
        };

        var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;
        r = hue2rgb(p, q, h + 1 / 3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1 / 3);
      }

      var toHex = function toHex(x) {
        var hex = Math.round(x * 255).toString(16);
        return hex.length === 1 ? '0' + hex : hex;
      };

      return "#".concat(toHex(r)).concat(toHex(g)).concat(toHex(b));
    }
  }, {
    key: "hex_to_hsl",
    value: function hex_to_hsl(hex, is_formatted) {
      var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
      var r = parseInt(result[1], 16);
      var g = parseInt(result[2], 16);
      var b = parseInt(result[3], 16);
      r /= 255, g /= 255, b /= 255;
      var max = Math.max(r, g, b),
          min = Math.min(r, g, b);
      var h,
          s,
          l = (max + min) / 2;

      if (max == min) {
        h = s = 0;
      } else {
        var d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

        switch (max) {
          case r:
            h = (g - b) / d + (g < b ? 6 : 0);
            break;

          case g:
            h = (b - r) / d + 2;
            break;

          case b:
            h = (r - g) / d + 4;
            break;
        }

        h /= 6;
      }

      s = s * 100;
      s = Math.round(s);
      l = l * 100;
      l = Math.round(l);
      h = Math.round(360 * h);
      var colorInHSL = [h, s, l];

      if (!is_formatted) {
        return colorInHSL;
      } else {
        return "hsl(".concat(h, ", ").concat(s, "%, ").concat(l, "%)");
      }
    }
  }]);
  return Colors;
}();

DOMTokenList.prototype.toggleMatch = function (match_str, new_class, add) {
  var _this28 = this;

  var state = {};
  var replaced_array = [];
  this.forEach(function (class_name) {
    if (-1 != class_name.indexOf(match_str)) {
      replaced_array.push(class_name);

      _this28.replace(class_name, new_class);

      state["added"] = true;
    }
  });
  state["replaced"] = replaced_array;

  if (add && 0 === replaced_array.length) {
    this.add(new_class);
    state["added"] = true;
  }

  return state;
};

String.prototype.splitWords = function () {
  return this.replace(/[^a-zA-Z0-9-]+/g, " ").replace(/([a-z])([A-Z])/g, '$1 $2').replace(/\b([A-Z]+)([A-Z])([a-z])/, '$1 $2$3').replace(/^./, function (str) {
    return str.toUpperCase();
  });
};

Number.prototype.countDecimals = function () {
  if (Math.floor(this.valueOf()) === this.valueOf()) return 0;
  var num = 0;

  if (this.toString().includes("e-")) {
    num = Number(this.toString().split("e-")[1]);

    if (this.toString().includes(".")) {
      num += this.toString().split(".")[1].length;
    }
  } else if (this.toString().includes(".")) {
    num = this.toString().split(".")[1].length;
  }

  return num;
};

var FloatingPointMath =
/*#__PURE__*/
function () {
  function FloatingPointMath() {
    (0, _classCallCheck2.default)(this, FloatingPointMath);
  }

  (0, _createClass2.default)(FloatingPointMath, [{
    key: "multiply",
    value: function multiply(num1, num2) {
      var mult = Math.pow(10, Math.max(num1.countDecimals(), num2.countDecimals()));
      var product = num1 * (num2 * mult) / mult;
      return product;
    }
  }, {
    key: "divide",
    value: function divide(num1, num2) {
      var mult = Math.pow(10, Math.max(num1.countDecimals(), +num2.countDecimals()));
      var quotient = num1 / num2 * mult / mult;
      return quotient;
    }
  }, {
    key: "add",
    value: function add(num1, num2) {
      var mult = Math.pow(10, Math.max((+num1).countDecimals(), (+num2).countDecimals()));
      var total = (num1 * mult + num2 * mult) / mult;
      return total;
    }
  }, {
    key: "subtract",
    value: function subtract(num1, num2) {
      var mult = Math.pow(10, Math.max((+num1).countDecimals(), (+num2).countDecimals()));
      var total = (num1 * mult - num2 * mult) / mult;
      return total;
    }
  }]);
  return FloatingPointMath;
}();

var Utilities =
/*#__PURE__*/
function () {
  function Utilities() {
    (0, _classCallCheck2.default)(this, Utilities);
    this.id_list = {};
  }

  (0, _createClass2.default)(Utilities, [{
    key: "has_uniform_values",
    value: function has_uniform_values(arr) {
      return !arr.some(function (value, index, arr) {
        return arr[0] !== value && "none" !== value;
      });
    }
  }, {
    key: "string_to_number",
    value: function string_to_number(number_str) {
      if ("number" === typeof number_str) {
        return number_str;
      }

      var filtered_num_zero_indx = number_str.substring(0, 1).replace(/[^0-9.-]/g, "");
      var filtered_num_substr = number_str.substring(1).replace(/[^0-9.e]/g, "");
      var final_num_str = filtered_num_zero_indx[0] + filtered_num_substr;
      var number = parseFloat(final_num_str);
      return number;
    }
  }, {
    key: "format_number",
    value: function format_number(num, precision, increment) {
      var p = precision || 1;
      var i = increment || 0;
      var fixed_precision = Math.round((num + i).toFixed(p) * Math.pow(10, p)) / Math.pow(10, p);

      if (fixed_precision.toString().includes(".")) {
        return fixed_precision.toString();
      } else {
        return fixed_precision.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
      }
    }
  }, {
    key: "format_numeric_label",
    value: function format_numeric_label(num_str, multiple) {
      var num_arr = num_str.split(",");
      var filtered_num = num_str.replace(/\D/g, "");
      var units = ["K", "M", "B", "T"];
      var numeric_label = num_str;

      if (num_arr.length >= 2 && num_arr.length <= 5) {
        if (Math.pow(1000, num_arr.length - 1) > multiple) {
          numeric_label = num_arr[0] + "." + num_arr[1] + units[num_arr.length - 2];
        } else {
          numeric_label = num_arr[0] + units[num_arr.length - 2];
        }
      } else if (num_arr.length > 5) {
        numeric_label = num_arr[0][0] + "." + num_arr[0].substring(1) + num_arr[1][0] + "e" + (filtered_num.length - 1);
      }

      return numeric_label;
    }
  }, {
    key: "get_numeric_bounds",
    value: function get_numeric_bounds(arr, axis, axis_options) {
      var bounds = {};
      bounds.min = Math.min.apply(Math, (0, _toConsumableArray2.default)(arr));
      bounds.max = Math.max.apply(Math, (0, _toConsumableArray2.default)(arr));
      bounds.total = arr.reduce(function (total, val) {
        return float.add(total, val);
      }, 0);
      bounds.spread = float.subtract(bounds.max, bounds.min);
      var user_input = false;
      var axis_values = {};

      if ("x" === axis || "y" === axis) {
        if (!isNaN(parseFloat(axis_options[axis].start_value)) || !isNaN(parseFloat(axis_options[axis].end_value)) || !isNaN(parseFloat(axis_options[axis].interval_value))) {
          user_input = true;
          axis_values.start_value = axis_options[axis].start_value;
          axis_values.end_value = axis_options[axis].end_value;
          axis_values.interval_value = axis_options[axis].interval_value;
        }
      }

      var label_bounds = this.normalize_bounds(bounds.min, bounds.max, user_input, axis_values);
      bounds.label_min = label_bounds.min_norm;
      bounds.label_max = label_bounds.max_norm;
      bounds.mult = label_bounds.mult;
      bounds.interval = label_bounds.interval;
      bounds.max_chars = label_bounds.max_chars;
      bounds.tick_label_array = label_bounds.tick_labels_arr;
      return bounds;
    }
  }, {
    key: "normalize_bounds",
    value: function normalize_bounds(min, max, user_input, axis_options) {
      var delimiter = Math.pow(10, max.toString().length - 1);
      var change_numbering = false;
      var range = float.subtract(max, min);

      if ((0 > min || range < min / 8) && 0 !== range) {
        change_numbering = true;
      }

      var y_axis_range = max;

      if (change_numbering) {
        y_axis_range = range;
      }

      var num_str = y_axis_range.toString();
      var integer_decimal_array = num_str.split(".");
      var mult = Math.pow(10, integer_decimal_array[0].length - 1);
      var num_zero = 0;
      var exponent = 0;

      if (num_str.includes("e-")) {
        exponent = Number(num_str.split("e")[1]);
        mult = Math.pow(10, exponent);
      }

      if (0 === +integer_decimal_array[0] && integer_decimal_array[1]) {
        for (var b = 0; b < integer_decimal_array[1].length; ++b) {
          if (+integer_decimal_array[1][b] === 0) {
            num_zero++;
          } else {
            break;
          }
        }

        mult = Math.pow(10, -1 * num_zero - 1);
        num_str = integer_decimal_array[1].substring(num_zero);
      }

      if (1 < num_str.length && 10 === +num_str.slice(0, 2)) {
        mult = mult / 10;
      } else if (5 > +num_str[0]) {
        mult = mult / 2;

        if (2 > +num_str[0]) {
          mult = mult * 0.8;
        }
      }

      var range_norm = Math.ceil(max / mult) * mult - Math.floor(min / mult) * mult;
      var interval = Math.ceil(y_axis_range / mult);
      var max_norm = float.multiply(interval, mult);
      var min_norm = 0;

      if (change_numbering) {
        max_norm = float.multiply(Math.ceil(float.divide(max, mult) + 0.1), mult);
        min_norm = float.multiply(Math.floor(float.divide(min, mult) - 0.1), mult);
        interval = Math.ceil(float.subtract(max_norm, min_norm) / mult);
      }

      if (0 > max && range > Math.abs(max) / 8) {
        max_norm = 0;
        min_norm = float.multiply(Math.floor(float.divide(min, mult) - 0.1), mult);
        interval = Math.ceil(float.subtract(max_norm, min_norm) / mult);
      }

      if (user_input) {
        if (axis_options.start_value || axis_options.start_value === 0) {
          min_norm = axis_options.start_value;
        }

        if (axis_options.end_value || axis_options.end_value === 0) {
          max_norm = axis_options.end_value;
        }

        if (axis_options.interval_value || axis_options.interval_value === 0) {
          mult = axis_options.interval_value;
        } else {
          y_axis_range = float.subtract(max_norm, min_norm);
          num_str = y_axis_range.toString();
          integer_decimal_array = num_str.split(".");
          mult = Math.pow(10, integer_decimal_array[0].length - 1);
          num_zero = 0;
          exponent = 0;

          if (num_str.includes("e-")) {
            exponent = Number(num_str.split("e")[1]);
            mult = Math.pow(10, exponent);
          }

          if (+integer_decimal_array[0] === 0) {
            for (var _b = 0; _b < integer_decimal_array[1].length; ++_b) {
              if (+integer_decimal_array[1][_b] === 0) {
                num_zero++;
              } else {
                break;
              }
            }

            mult = Math.pow(10, -1 * num_zero - 1);
            num_str = integer_decimal_array[1].substring(num_zero);
          }

          if (5 > +num_str[0]) {
            mult = mult / 2;

            if (2 > +num_str[0]) {
              mult = mult * .8;
            }
          }

          mult = float.divide(y_axis_range, 10);
        }

        interval = Math.ceil(float.subtract(max_norm, min_norm) / mult);
      }

      var precision = mult.countDecimals();
      var max_chars = 0;
      var tick_label_font_length = 0;
      var tick_labels_arr = [];

      for (var y = 0, y_len = interval; y_len >= y; ++y) {
        var tick_value = float.add(min_norm, float.multiply(mult, y));

        if (y === y_len) {
          max_norm = tick_value;
        }

        var tick_title = this.format_number(tick_value, precision);
        var tick_label = this.format_numeric_label(tick_title, mult);

        if (max_chars < tick_label.length) {
          max_chars = tick_label.length;
        }

        tick_labels_arr.push(tick_label);
      }

      return {
        min_norm: min_norm,
        max_norm: max_norm,
        interval: interval,
        mult: mult,
        max_chars: max_chars,
        tick_labels_arr: tick_labels_arr
      };
    }
  }, {
    key: "get_aggregate_bounds",
    value: function get_aggregate_bounds(arr, axis, axis_options) {
      var bounds = arr[0];
      arr.forEach(function (each_bounds) {
        bounds.min = Math.min(bounds.min, each_bounds.min);
        bounds.max = Math.max(bounds.max, each_bounds.max);
      });
      bounds.spread = float.subtract(bounds.max, bounds.min);
      var user_input = false;
      var axis_values = {};

      if ("x" === axis || "y" === axis) {
        if (!isNaN(parseFloat(axis_options[axis].start_value)) || !isNaN(parseFloat(axis_options[axis].end_value)) || !isNaN(parseFloat(axis_options[axis].interval_value))) {
          user_input = true;
          axis_values.start_value = axis_options[axis].start_value;
          axis_values.end_value = axis_options[axis].end_value;
          axis_values.interval_value = axis_options[axis].interval_value;
        }
      }

      var label_bounds = this.normalize_bounds(bounds.min, bounds.max, user_input, axis_values);
      bounds.label_min = label_bounds.min_norm;
      bounds.label_max = label_bounds.max_norm;
      bounds.mult = label_bounds.mult;
      bounds.interval = label_bounds.interval;
      bounds.max_chars = label_bounds.max_chars;
      bounds.tick_label_array = label_bounds.tick_labels_arr;
      return bounds;
    }
  }, {
    key: "get_max_characters",
    value: function get_max_characters(arr) {
      var longest = arr.sort(function (a, b) {
        return b.length - a.length;
      })[0];
      return uid;
    }
  }, {
    key: "generate_unique_id",
    value: function generate_unique_id(base_id) {
      base_id = base_id.replace(/\s+/g, "_").replace(/[\W]+/g, "");
      var i = 0;
      var uid = base_id;

      while (this.id_list[uid] || null !== document.getElementById(uid)) {
        uid = base_id + "-" + ++i;
      }

      this.id_list[uid] = true;
      return uid;
    }
  }, {
    key: "get_transform_offset",
    value: function get_transform_offset(el1, el2) {
      var el1_translate = this.get_transform_translate(el1);
      var el2_translate = this.get_transform_translate(el2);
      return {
        x: el1_translate.x - el2_translate.x,
        y: el1_translate.y - el2_translate.y
      };
    }
  }, {
    key: "get_transform_translate",
    value: function get_transform_translate(el) {
      var transform_val = el.getAttribute("transform");
      var components = transform_val.split(")");

      for (var t = 0, t_len = components.length; t_len > t; ++t) {
        var each_component = components[t];

        if (-1 != each_component.indexOf("translate")) {
          var vals = each_component.replace("translate(", "").split(",");
          return {
            x: +vals[0],
            y: +vals[1]
          };
        }
      }
    }
  }, {
    key: "get_local_coords",
    value: function get_local_coords(x, y, el, root) {
      var p = root.createSVGPoint();
      p.x = x;
      p.y = y;
      p = p.matrixTransform(el.getScreenCTM().inverse());
      p.x = +p.x.toFixed(2);
      p.y = +p.y.toFixed(2);
      return p;
    }
  }, {
    key: "compose_title_from_facets",
    value: function compose_title_from_facets(facets) {
      return facets.reduce(function (label_str, facet) {
        return "" !== label_str ? "".concat(label_str, ", ").concat(facet.label) : facet.label;
      }, "");
    }
  }]);
  return Utilities;
}();

var DataTools =
/*#__PURE__*/
function () {
  function DataTools() {
    (0, _classCallCheck2.default)(this, DataTools);
  }

  (0, _createClass2.default)(DataTools, [{
    key: "get_values",
    value: function get_values(facet, type) {
      type = type || "norm";
      return facet.datapoints.map(function (datapoint) {
        return datapoint.value[type];
      });
    }
  }, {
    key: "get_norm_values",
    value: function get_norm_values(facet, type) {
      return facet.datapoints.map(function (datapoint) {
        return datapoint.value.norm;
      });
    }
  }, {
    key: "get_raw_values",
    value: function get_raw_values(facet) {
      return facet.datapoints.map(function (datapoint) {
        return datapoint.value.raw;
      });
    }
  }, {
    key: "get_date_details",
    value: function get_date_details(val) {
      var val_array = val.split('T');
      var format_val = val_array[0].replace(/-/g, "/");

      if (val_array[1]) {
        format_val += " ".concat(val_array[1]);
      }

      var date = new Date(format_val);
      var date_obj = {
        month: new Intl.DateTimeFormat('en-US', {
          month: 'long'
        }).format(date),
        month_abbr: new Intl.DateTimeFormat('en-US', {
          month: 'short'
        }).format(date),
        day: date.getUTCDate(),
        year: date.getUTCFullYear(),
        weekday: new Intl.DateTimeFormat('en-US', {
          weekday: 'long'
        }).format(date)
      };
      return date_obj;
    }
  }, {
    key: "get_datatype",
    value: function get_datatype(val) {
      var datatype_value = {
        datatype: "numeric",
        norm_value: val
      };

      if ("number" !== typeof datatype_value.norm_value) {
        datatype_value.norm_value = datatype_value.norm_value.trim();

        if (!val || "" === val.replace(/\s+/g, "")) {
          datatype_value.datatype = "none";
        } else {
          if (this.detect_date(datatype_value.norm_value)) {
            datatype_value.datatype = "date";
            var date = datatools.get_date_details(datatype_value.norm_value);
            datatype_value.norm_value = "".concat(date.month_abbr, " ").concat(date.day, ", ").concat(date.year);
          } else {
            var num = utils.string_to_number(val);

            if (Number.isNaN(num)) {
              datatype_value.datatype = "category";
            } else {
              var num_len = num.toString().length;
              var val_len = val.length;

              if (num_len > val_len / 5) {
                datatype_value.datatype = "numeric";
                datatype_value.norm_value = num;
              } else {
                datatype_value.datatype = "category";
              }
            }
          }
        }
      }

      return datatype_value;
    }
  }, {
    key: "group_facet_keys_by_datatype",
    value: function group_facet_keys_by_datatype(facets) {
      var datatype_map = new Map();
      Object.keys(facets).forEach(function (facet) {
        var datatype = function (facet) {
          return facets[facet].datatype;
        }(facet);

        if (!datatype_map.has(datatype)) {
          datatype_map.set(datatype, [facet]);
        } else {
          datatype_map.get(datatype).push(facet);
        }
      });
      return datatype_map;
    }
  }, {
    key: "group_facet_keys_by_axis",
    value: function group_facet_keys_by_axis(facets) {
      var axis_map = new Map();
      Object.keys(facets).forEach(function (facet) {
        var axis = function (facet) {
          return facets[facet].axis;
        }(facet);

        if (!axis_map.has(axis)) {
          axis_map.set(axis, [facet]);
        } else {
          axis_map.get(axis).push(facet);
        }
      });
      return axis_map;
    }
  }, {
    key: "get_selected_facet_keys",
    value: function get_selected_facet_keys(facets) {
      var selected_facets = Object.keys(facets).filter(function (facet) {
        return facets[facet].selected;
      });
      return selected_facets;
    }
  }, {
    key: "filter_selected_facet_keys_from_datatype_map",
    value: function filter_selected_facet_keys_from_datatype_map(facets, datatype_map) {
      var selected_facet_keys = this.get_selected_facet_keys(facets);
      var _iteratorNormalCompletion4 = true;
      var _didIteratorError4 = false;
      var _iteratorError4 = undefined;

      try {
        for (var _iterator4 = datatype_map.entries()[Symbol.iterator](), _step4; !(_iteratorNormalCompletion4 = (_step4 = _iterator4.next()).done); _iteratorNormalCompletion4 = true) {
          var _step4$value = (0, _slicedToArray2.default)(_step4.value, 2),
              datatype = _step4$value[0],
              facet_keys = _step4$value[1];

          facet_keys = facet_keys.filter(function (facet_key) {
            return !selected_facet_keys.includes(facet_key);
          });

          if (facet_keys.length) {
            datatype_map.set(datatype, facet_keys);
          } else {
            datatype_map.delete(datatype);
          }
        }
      } catch (err) {
        _didIteratorError4 = true;
        _iteratorError4 = err;
      } finally {
        try {
          if (!_iteratorNormalCompletion4 && _iterator4.return != null) {
            _iterator4.return();
          }
        } finally {
          if (_didIteratorError4) {
            throw _iteratorError4;
          }
        }
      }
    }
  }, {
    key: "filter_facet_keys_selection_in_key_map",
    value: function filter_facet_keys_selection_in_key_map(facets, key_map, include) {
      var selected_facet_keys = this.get_selected_facet_keys(facets);
      var _iteratorNormalCompletion5 = true;
      var _didIteratorError5 = false;
      var _iteratorError5 = undefined;

      try {
        for (var _iterator5 = key_map.entries()[Symbol.iterator](), _step5; !(_iteratorNormalCompletion5 = (_step5 = _iterator5.next()).done); _iteratorNormalCompletion5 = true) {
          var _step5$value = (0, _slicedToArray2.default)(_step5.value, 2),
              category = _step5$value[0],
              facet_keys = _step5$value[1];

          if (!include) {
            facet_keys = facet_keys.filter(function (facet_key) {
              return !selected_facet_keys.includes(facet_key);
            });
          } else {
            facet_keys = facet_keys.filter(function (facet_key) {
              return selected_facet_keys.includes(facet_key);
            });
          }

          if (facet_keys.length) {
            key_map.set(category, facet_keys);
          } else {
            key_map.delete(category);
          }
        }
      } catch (err) {
        _didIteratorError5 = true;
        _iteratorError5 = err;
      } finally {
        try {
          if (!_iteratorNormalCompletion5 && _iterator5.return != null) {
            _iterator5.return();
          }
        } finally {
          if (_didIteratorError5) {
            throw _iteratorError5;
          }
        }
      }
    }
  }, {
    key: "detect_date",
    value: function detect_date(str) {
      var MMDDYYY = /^(?:(?:(?:0?[13578]|10|12)(-|\/)(?:0?[1-9]|[12]\d?|3[01]?)\1|(?:0?[469]|11)(-|\/)(?:0?[1-9]|[12]\d?|3[0]?)\2|0?2(-|\/)(?:0?[1-9]|1\d|2[0-8])\3)(?:19[2-9]\d{1}|20[01]\d{1}|\d{2}))$/;
      var YYYYMMDD = /^(?:(?:(?:(?:(?:[1-9]\d)(?:0[48]|[2468][048]|[13579][26])|(?:(?:[2468][048]|[13579][26])00))(\/|-|\.)(?:0?2\1(?:29)))|(?:(?:[1-9]\d{3})(\/|-|\.)(?:(?:(?:0?[13578]|1[02])\2(?:31))|(?:(?:0?[13-9]|1[0-2])\2(?:29|30))|(?:(?:0?[1-9])|(?:1[0-2]))\2(?:0?[1-9]|1\d|2[0-8])))))$/;
      var is_date = false;

      if (str.match(MMDDYYY) || str.match(YYYYMMDD)) {
        is_date = true;
      }

      if (!is_date && str.includes(":")) {
        is_date = true;
      }

      return is_date;
    }
  }]);
  return DataTools;
}();

var utils = new Utilities();
var float = new FloatingPointMath();
var datatools = new DataTools();
var chart_styles = {
  'svg': {
    'font-family': 'Lato-Bold, sans-serif',
    'font-size': '15px',
  },
  '#fizz_chart_frame': {
    'fill': 'none',
    'stroke': 'none'
  },
  '.y_axis_line': {
    'fill': 'none',
    'stroke': 'hsl(0, 0%, 0%)',
    'stroke-width': '2px',
    'stroke-linecap': 'round'
  },
  '.x_axis_line': {
    'fill': 'none',
    'stroke': 'hsl(0, 0%, 0%)',
    'opacity': '1',
    'stroke-width': '2px',
    'stroke-linecap': 'round'
  },
  '.axis_label': {
    'text-anchor': 'middle',
    'fill': 'hsl(0, 0%, 0%)',
    'stroke': 'none'
  },
  '.axis_label_y': {
    'text-anchor': 'end',
    'fill': 'hsl(0, 0%, 0%)',
    'stroke': 'none'
  },
  '.tickmark_y': {
    'opacity': '0.2'
  },
  '.tickmark_y_0': {
    'fill': 'hsl(270, 50%, 50%)',
    'stroke': 'hsl(270, 50%, 50%)',
    'opacity': '1',
    'stroke-width': '2px',
    'stroke-linecap': 'round'
  },
  '.tick_group_y:hover, .tick_group_y:hover .tickmark_y': {
    'font-weight': 'bold',
    'opacity': '1'
  },
  '.tickmark_x': {
    'opacity': '0.2'
  },
  '.tickmark_x_0': {
    'fill': 'hsl(270, 50%, 50%)',
    'stroke': 'hsl(270, 50%, 50%)',
    'opacity': '1',
    'stroke-width': '2px',
    'stroke-linecap': 'round'
  },
  '.tick_group_x:hover, .tick_group_x:hover .tickmark_x': {
    'font-weight': 'bold',
    'opacity': '1'
  },
  '.radial_label': {
    'fill': 'hsl(222, 34%, 23%)'
  },
  '.radial_label_right': {
    'text-anchor': 'start'
  },
  '.radial_label_left': {
    'text-anchor': 'end'
  },
  '.bar:hover, .bar:focus': {
    'fill': 'hsl(270, 50%, 65%)',
    'outline': '2px auto -webkit-focus-ring-color'
  },
  '.datapoint_background': {
    'fill': 'none',
    'stroke': 'none',
    'pointer-events': 'all',
    'outline': 'none'
  },
  '.data_line': {
    'fill': 'none',
    'stroke': 'hsl(270, 50%, 50%)',
    'stroke-width': '3px',
    'stroke-linecap': 'round'
  },
  '.data_area': {
    'stroke': 'hsl(270, 50%, 50%)',
    'stroke-width': '3px',
    'stroke-linecap': 'round',
    'fill': 'none'
  },
  '.data_area_background': {
    'stroke': 'none',
    'fill': 'hsl(270, 50%, 50%)',
    'fill-opacity': '0.8'
  },
  '.trendline': {
    'stroke': 'red',
    'stroke-width': '3px',
    'fill': 'none'
  },
  '.center_label': {
    'font-size': '7rem',
    'text-anchor': 'middle'
  },
  '.center_label > tspan.subtext': {
    'font-size': '40px',
    'text-anchor': 'middle'
  },
  'g.datapoint g.datapoint_popup rect': {
    'fill': 'hsl(0, 0%, 25%)'
  },
  'g.datapoint g.datapoint_popup text': {
    'fill': 'white',
    'font-size': '1rem',
    'text-anchor': 'middle'
  },
  'g.datapoint g.datapoint_popup': {
    'stroke': 'none',
    'opacity': '0.0',
    'transition': 'opacity 0.3s ease-in-out'
  },
  'g.datapoint:focus g.datapoint_popup, g.datapoint:hover g.datapoint_popup': {
    'opacity': '1.0',
    'transition': 'opacity 0.3s ease-in-out'
  },
  'g.datapoint g.datapoint_popup .desc': {
    'opacity': '0.0'
  },
  'g.datapoint:focus g.datapoint_popup .desc, g.datapoint:hover g.datapoint_popup .desc': {
    'opacity': '1.0',
    'transition': 'opacity 0.3s ease-in-out 2s'
  },
  '.chart_title': {
    'font-size': '30px !important',
    'text-anchor': 'start !important',
    'fill': '#26324e !important',
    'text-transform': 'uppercase !important',
    'font-family': 'Lato-Bold, sans-serif !important'
  }
};
var FizzSettings = {
  options: {
    chart: {
      title: {
        text: null,
        margin: 3,
        font_size: 18
      },
      area: {
        x: 0,
        y: 0,
        width: 500,
        height: 500
      },
      padding: 15,
      chart_type: 'bar'
    },
    multiseries: {
      type: 'single'
    },
    axis: {
      min_interval: 25,
      datapoint_margin: 3,
      x: {
        title: {
          text: null,
          margin: 8,
          font_size: 15
        },
        tick: {
          margin: 3,
          font_size: 13,
          opacity: 1,
          stroke_width: 2,
          stroke_linecap: 'round',
          length: 10
        },
        values: {
          start: null,
          end: null,
          interval: null
        },
        stroke_width: 2,
        stroke_linecap: 'round'
      },
      y: {
        title: {
          text: null,
          margin: 8,
          font_size: 15
        },
        tick: {
          margin: 3,
          font_size: 13,
          opacity: 1,
          stroke_width: 2,
          stroke_linecap: 'round',
          length: 10
        },
        values: {
          start: null,
          end: null,
          interval: null
        },
        stroke_width: 2,
        stroke_linecap: 'round'
      },
      r: {
        title: {
          text: null,
          margin: 8,
          font_size: 15,
          render: 'none'
        }
      }
    },
    bar: {
      bar_width_min: 10
    },
    line: {
      line_width: 5,
      base_symbol_size: 10
    },
    scatterplot: {
      clusters: {
        active: false,
        count: {
          value: 2,
          min: 0,
          max: 30
        }
      },
      trendline: {
        active: false,
        linear: true,
        polynomial: false,
        polynomial_degree: {
          value: 2,
          min: 0,
          max: 5
        }
      }
    },
    radial: {
      label: {
        margin: 3,
        font_size: 15
      },
      radius: {
        outer: 'auto',
        inner: 0.5
      }
    },
    pie: {
      label: {
        margin: 3,
        font_size: 15
      },
      radius: {
        outer: 'auto',
        inner: null
      }
    },
    donut: {
      label: {
        margin: 3,
        font_size: 15
      },
      radius: {
        outer: 'auto',
        inner: 0.5
      }
    },
    gauge: {
      label: {
        margin: 3,
        font_size: 15
      },
      center_label: {
        is_render: true
      },
      radius: {
        outer: 'auto',
        inner: 0.75
      }
    },
    map: {
      map_id: null,
      padding: 0.01
    },
    colors: {
      primary: 'hsl(270, 50%, 50%)',
      accent: 'hsl(270, 50%, 25%)',
      active: 'hsl(270, 50%, 65%)',
      palette: null
    }
  },
  facets: {}
};