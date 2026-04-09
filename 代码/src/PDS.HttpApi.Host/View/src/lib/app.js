import { GetConfiguration } from "@/api/system";
import store from '@/store';
export default {
    loadAsync: async function() {
        let configuration = await GetConfiguration();
        store.commit('global/setGlobalState', {
            user: configuration.currentUser,
            authed: configuration.auth.grantedPolicies
        });
    }
}