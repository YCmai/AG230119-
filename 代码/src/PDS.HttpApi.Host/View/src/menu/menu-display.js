import localizer from "@/lib/localizer";
import locale from '@/localize/locale';
import store from '@/store';
export default function(key) {
    const i18n = localizer(locale, store.state.global.culture);
    return function() { return i18n.t(key) };
}