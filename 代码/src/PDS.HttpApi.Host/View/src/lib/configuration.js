import plugins from "@/lib/spin";
import { GetConfiguration } from "@/api/app";
import store from "@/store";
export default async function() {
    let spin = plugins.Spin({ text: "Loading Configurations..." });
    try {
        let configuration = await GetConfiguration();
        store.commit("global/setUser", configuration.currentUser);
        store.commit(
            "global/setCulture",
            configuration.localization.currentCulture.cultureName
        );
        store.commit("global/setAuthed", configuration.auth.grantedPolicies);
    } catch (err) {
        console.log(err); 
    } finally {
        spin.close();
    }
}