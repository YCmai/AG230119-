<template>
  <a-layout class="default-layout container" id="container">
    <a-layout-content class="default-layout-content calc-100">
      <a-row class="calc-100">
        <a-col :span="16" class="calc-100">
          <role-list @auth="auth"></role-list>
        </a-col>
        <a-col :span="8" class="calc-100">
          <role-permission
            ref="permission"
            v-if="isGranted('AbpIdentity.Roles.ManagePermissions')"
          ></role-permission>
        </a-col>
      </a-row>
    </a-layout-content>
  </a-layout>
</template>
<script>
import RoleList from "./RoleManager.vue";
import RolePermission from "./RolePermission.vue";
import component from "@/lib/base";
export default {
  name: "RoleIndex",
  mixins: [component],
  components: { RoleList, RolePermission },
  methods: {
    auth: function (roleName) {
      this.$refs.permission.load(roleName);
    },
  },
};
</script>
<style lang="less" scoped>
.container {
  position: relative;
}
.calc-100 {
  height: calc(100%);
}
</style>
