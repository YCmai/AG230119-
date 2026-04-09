import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import antd from 'ant-design-vue';
import 'ant-design-vue/dist/antd.css';
import spin from '@/lib/spin';
import sider from '@/lib/sider/Index.vue'
// import VueI18n from 'vue-i18n';
import locale from '@/lib/base-locale';
import localizer from "@/lib/localizer";
const i18n = localizer(locale, store.state.global.culture);
Vue.use(antd);
Vue.use(spin);
Vue.component('menu-sider', sider);
Vue.config.productionTip = false
    //Vue.use(VueI18n);
    // const i18n = new VueI18n({
    //     locale: store.state.global.culture,
    //     messages: {
    //         'zh-Hans': locale['zh-Hans'],
    //         'en': locale.en
    //     },
    //     silentTranslationWarn: true
    // });

new Vue({
    router,
    store,
    i18n,
    render: function(h) { return h(App) }
}).$mount('#app')