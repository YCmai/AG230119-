import SpinVue from './Spin.vue';
import Vue from 'vue';
const PopupManager = {
    zIndex: 2000,
    nextIndex() {
        return PopupManager.zIndex++;
    }
};
const SpinConstructor = Vue.extend(SpinVue);


const defaults = {
    text: null,
    fullscreen: true,
    body: false,
    lock: false,
    customClass: ''
};
let fullscreenLoading;

SpinConstructor.prototype.originalPosition = '';
SpinConstructor.prototype.originalOverflow = '';

function afterLeave(instance, callback, speed = 300, once = false) {
    if (!instance || !callback) throw new Error('instance & callback is required');
    let called = false;
    const afterLeaveCallback = function() {
        if (called) return;
        called = true;
        if (callback) {
            callback.apply(null, arguments);
        }
    };
    if (once) {
        instance.$once('after-leave', afterLeaveCallback);
    } else {
        instance.$on('after-leave', afterLeaveCallback);
    }
    setTimeout(() => {
        afterLeaveCallback();
    }, speed + 100);
};
SpinConstructor.prototype.close = function() {
    if (this.fullscreen) {
        fullscreenLoading = undefined;
    }
    afterLeave(this, _ => {
        const target = this.fullscreen || this.body ?
            document.body :
            this.target;
        removeClass(target, 'el-loading-parent--relative');
        removeClass(target, 'el-loading-parent--hidden');
        if (this.$el && this.$el.parentNode) {
            this.$el.parentNode.removeChild(this.$el);
        }
        this.$destroy();
    }, 300);
    this.visible = false;
};
const SPECIAL_CHARS_REGEXP = /([\:\-\_]+(.))/g;
const MOZ_HACK_REGEXP = /^moz([A-Z])/;
const camelCase = function(name) {
    return name.replace(SPECIAL_CHARS_REGEXP, function(_, separator, letter, offset) {
        return offset ? letter.toUpperCase() : letter;
    }).replace(MOZ_HACK_REGEXP, 'Moz$1');
};

function addClass(el, cls) {
    if (!el) return;
    var curClass = el.className;
    var classes = (cls || '').split(' ');

    for (var i = 0, j = classes.length; i < j; i++) {
        var clsName = classes[i];
        if (!clsName) continue;

        if (el.classList) {
            el.classList.add(clsName);
        } else if (!hasClass(el, clsName)) {
            curClass += ' ' + clsName;
        }
    }
    if (!el.classList) {
        el.setAttribute('class', curClass);
    }
};
const addStyle = (options, parent, instance) => {
    let maskStyle = {};
    if (options.fullscreen) {
        instance.originalPosition = getStyle(document.body, 'position');
        instance.originalOverflow = getStyle(document.body, 'overflow');
        maskStyle.zIndex = PopupManager.nextIndex();
    } else if (options.body) {
        instance.originalPosition = getStyle(document.body, 'position');
        ['top', 'left'].forEach(property => {
            let scroll = property === 'top' ? 'scrollTop' : 'scrollLeft';
            maskStyle[property] = options.target.getBoundingClientRect()[property] +
                document.body[scroll] +
                document.documentElement[scroll] +
                'px';
        });
        ['height', 'width'].forEach(property => {
            maskStyle[property] = options.target.getBoundingClientRect()[property] + 'px';
        });
    } else {
        instance.originalPosition = getStyle(parent, 'position');
    }
    Object.keys(maskStyle).forEach(property => {
        instance.$el.style[property] = maskStyle[property];
    });
};
const getStyle = function(element, styleName) {
    if (!element || !styleName) return null;
    styleName = camelCase(styleName);
    if (styleName === 'float') {
        styleName = 'cssFloat';
    }
    try {
        var computed = document.defaultView.getComputedStyle(element, '');
        return element.style[styleName] || computed ? computed[styleName] : null;
    } catch (e) {
        return element.style[styleName];
    }
};

function removeClass(el, cls) {
    if (!el || !cls) return;
    var classes = cls.split(' ');
    var curClass = ' ' + el.className + ' ';

    for (var i = 0, j = classes.length; i < j; i++) {
        var clsName = classes[i];
        if (!clsName) continue;

        if (el.classList) {
            el.classList.remove(clsName);
        } else if (hasClass(el, clsName)) {
            curClass = curClass.replace(' ' + clsName + ' ', ' ');
        }
    }
    if (!el.classList) {
        el.setAttribute('class', trim(curClass));
    }
};

function merge(target) {
    for (let i = 1, j = arguments.length; i < j; i++) {
        let source = arguments[i] || {};
        for (let prop in source) {
            if (source.hasOwnProperty(prop)) {
                let value = source[prop];
                if (value !== undefined) {
                    target[prop] = value;
                }
            }
        }
    }

    return target;
};

const Spin = (options = {}) => {
    options = merge({}, defaults, options);
    if (typeof options.target === 'string') {
        options.target = document.querySelector(options.target);
    }
    options.target = options.target || document.body;
    if (options.target !== document.body) {
        options.fullscreen = false;
    } else {
        options.body = true;
    }
    if (options.fullscreen && fullscreenLoading) {
        return fullscreenLoading;
    }

    let parent = options.body ? document.body : options.target;
    let instance = new SpinConstructor({
        el: document.createElement('div'),
        data: options
    });

    addStyle(options, parent, instance);
    if (instance.originalPosition !== 'absolute' && instance.originalPosition !== 'fixed') {
        addClass(parent, 'el-loading-parent--relative');
    }
    if (options.fullscreen && options.lock) {
        addClass(parent, 'el-loading-parent--hidden');
    }
    parent.appendChild(instance.$el);
    Vue.nextTick(() => {
        instance.visible = true;
        instance.text = options.text;
    });
    if (options.fullscreen) {
        fullscreenLoading = instance;
    }
    return instance;
};

export default {
    install(Vue) {
        Vue.prototype.$spin = Spin;
        // Vue.use(directive);
        // Vue.prototype.$loading = service;
    },
    Spin
}