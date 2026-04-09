<template>
  <a-layout class="default-layout part-layout">
    <a-layout-header class="default-layout-header default-list-header" id="container">
      <a-row>
        <a-col :span="12" style="text-align: left">
          <span>{{ $t("title.title") }}</span>
          <span v-if="roleName">-</span>
          <span>{{ roleName }}</span>
        </a-col>
        <a-col :span="12">
          <a-button size="small" icon="save" @click="grant"
            >{{ $t("operation.auth") }}
          </a-button>
        </a-col>
      </a-row>
    </a-layout-header>
    <a-layout-content
      class="default-layout-content"
      style="min-height: 250px; overflow: scroll"
    >
      <a-spin :spinning="loading" :tip="$t('load.permission', [roleName])">
        <a-empty v-if="!roleName"></a-empty>
        <a-tree
          v-if="roleName"
          checkable
          v-model="granted"
          :tree-data="policies"
          :checkStrictly="true"
          :replaceFields="{ title: 'displayName', children: 'permissions', key: 'name' }"
        >
        </a-tree>
      </a-spin>
    </a-layout-content>
  </a-layout>
</template>
<script>
import locale from "@/localize/system/role";
import { Get, Grant } from "@/api/permission";
import component from "@/lib/base";
export default {
  name: "RolePermission",
  i18n: {
    messages: locale,
  },
  mixins: [component],
  data() {
    return {
      roleName: "",
      originals: [],
      policies: [],
      granted: { checked: [] },
      loading: false,
    };
  },
  methods: {
    load: async function (roleName) {
      let scope = this;
      scope.roleName = roleName;
      try {
        scope.loading = true;
        let result = await Get("R", roleName);
        let data = Build(result.groups);
        scope.originals = result.groups;
        scope.policies = data.policies;
        scope.granted.checked = data.granted;
      } catch (err) {
        scope.$message.error(err.message);
      } finally {
        scope.loading = false;
      }
    },
    grant: async function () {
      let scope = this;
      if (!scope.roleName) return;
      if (!(await scope.confirm(scope.$t("confirm.grant")))) return;
      scope.loading = true;
      try {
        let permissions = BuildResult(scope.originals, scope.granted.checked);
        await Grant("R", scope.roleName, permissions);
        scope.$message.success(scope.$t("save.success"));
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        scope.loading = false;
      }
    },
  },
};
function Build(groups) {
  let result = { policies: [], granted: [] };
  for (let i = 0; i < groups.length; i++) {
    let group = groups[i];
    let permissions = group.permissions;
    for (let i = 0; i < permissions.length; i++) {
      let permission = permissions[i];
      let parent = permissions.filter((m) => {
        return m.name == permission.parentName;
      })[0];
      if (parent) {
        if (!parent.permissions) parent.permissions = [];
        parent.permissions.push(permission);
      }
      if (permission.isGranted) result.granted.push(permission.name);
    }
    let groupChild = permissions.filter((m) => {
      return !m.parentName;
    });
    let permissionGroup = {
      displayName: group.displayName,
      name: group.name,
      permissions: [...groupChild],
      checkable: false,
    };
    result.policies.push(permissionGroup);
  }
  return result;
  // let result = { policies: [], granted: [] };
  // groups.forEach((group) => {
  //   let groupValue = {
  //     displayName: group.displayName,
  //     name: group.name,
  //     permissions: [],
  //     checkable: false,
  //   };
  //   let parents = group.permissions.filter((parent) => {
  //     return !parent.parentName;
  //   });
  //   parents.forEach((parent) => {
  //     let parentValue = {
  //       displayName: parent.displayName,
  //       name: parent.name,
  //       permissions: [],
  //       checkable: true,
  //     };
  //     if (parent.isGranted) result.granted.push(parent.name);
  //     let children = group.permissions.filter((child) => {
  //       return child.parentName == parent.name;
  //     });
  //     children.forEach((child) => {
  //       parentValue.permissions.push({
  //         displayName: child.displayName,
  //         name: child.name,
  //         checkable: true,
  //       });
  //       if (child.isGranted) result.granted.push(child.name);
  //     });
  //     groupValue.permissions.push(parentValue);
  //   });
  //   result.policies.push(groupValue);
  // });
  // return result;
}

function BuildResult(originals, checked) {
  let result = [];
  originals.forEach((group) => {
    group.permissions.forEach((permission) => {
      if (checked.indexOf(permission.name) > -1)
        result.push({ name: permission.name, isGranted: true });
      else result.push({ name: permission.name, isGranted: false });
    });
  });
  return result;
}
</script>
