import VueI18n from 'vue-i18n';
import Vue from 'vue';
Vue.use(VueI18n);
let i18n;
export default function(locale, culture) {
    if (!i18n)
        i18n = new VueI18n({
            locale: culture || 'zh-Hans',
            messages: {
                'zh-Hans': locale['zh-Hans'],
                'en': locale.en
            },
            silentTranslationWarn: true
        });
    return i18n;
};