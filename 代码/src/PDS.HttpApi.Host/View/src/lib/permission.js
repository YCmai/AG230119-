import store from '@/store';
export default function isGranted(permission) {
    if (!permission)
        return true;
    let global = store.state.global;
    if (!global || !global.authed)
        return false;
    let granted = global.authed[permission];
    return granted;
}