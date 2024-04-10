using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Weexuz.Modules
{
    public class Administration : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("delete", "Delete a messages in the channel")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Delete(int amount)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            amount = Math.Min(Math.Abs(amount), 512);

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_administration:1227216764396372059> Has been destroyed {amount} messages**")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("kick", "Kick a member from the server")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser member, string reason = "no provided")
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_hammer:1227216763024969738> {author.DisplayName} kicked {member.DisplayName}**")

                .WithDescription($"Kick with reason \"{reason}\"")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());

            try
            {
                EmbedBuilder message = new EmbedBuilder()
                    .WithColor(new Color(220, 20, 60))

                    .WithTitle($"**<:weexuz_message:1227216761397448785> You were kicked from {Context.Guild.Name}**")

                    .WithDescription($"Hello {member.DisplayName}, you were kicked from \"{Context.Guild.Name}\" for reason \"{reason}\"")

                    .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

                await member.SendMessageAsync(embed: message.Build());
            } catch { }

            await member.KickAsync(reason);
        }

        [SlashCommand("ban", "Ban a member from the server")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(SocketGuildUser member, string reason = "no provided")
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_hammer:1227216763024969738> {author.DisplayName} banned {member.DisplayName}**")

                .WithDescription($"Banned with reason \"{reason}\"")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());

            try
            {
                EmbedBuilder message = new EmbedBuilder()
                    .WithColor(new Color(220, 20, 60))

                    .WithTitle($"**<:weexuz_message:1227216761397448785> You were banned from {Context.Guild.Name}**")

                    .WithDescription($"Hello {member.DisplayName}, you were banned from \"{Context.Guild.Name}\" for reason \"{reason}\"")

                    .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

                await member.SendMessageAsync(embed: message.Build());
            }
            catch { }

            await member.BanAsync(reason: reason);
        }

        [SlashCommand("mute", "Mute member from the server")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(SocketGuildUser member, string reason = "no provided")
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_hammer:1227216763024969738> {author.DisplayName} muted {member.DisplayName}**")

                .WithDescription($"Mute with reason \"{reason}\"")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());

            IRole role = (IRole)Context.Guild.Roles.FirstOrDefault(role => role.Name == "Muted") ?? await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, new Color(220, 20, 60));

            foreach (SocketGuildChannel channel in Context.Guild.Channels)
                await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel));

            await member.AddRoleAsync(role);
        }

        [SlashCommand("tempmute", "Mute a member temporarily")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Tempmute(SocketGuildUser member, int minutes, string reason = "no provided")
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_hammer:1227216763024969738> {author.DisplayName} temporarily muted {member.DisplayName}**")

                .WithDescription($"Muted with reason \"{reason}\" for \"{minutes}\" minutes")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());

            IRole role = (IRole)Context.Guild.Roles.FirstOrDefault(role => role.Name == "Muted") ?? await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, new Color(220, 20, 60));

            foreach (SocketGuildChannel channel in Context.Guild.Channels)
                await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel));

            await member.AddRoleAsync(role);

            System.Timers.Timer timer = new System.Timers.Timer(minutes * 60 * 1000);
            timer.Elapsed += async (sender, e) =>
            {
                await member.RemoveRoleAsync(role);

                timer.Stop();
                timer.Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
            
            if (member.Roles.Contains(role))
                await member.RemoveRoleAsync(role);
        }

        [SlashCommand("unmute", "Unmute member from the server")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(SocketGuildUser member)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            IRole role = (IRole)Context.Guild.Roles.FirstOrDefault(role => role.Name == "Muted") ?? await Context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, new Color(220, 20, 60));

            if (member.Roles.Contains(role))
                await member.RemoveRoleAsync(role);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_hammer:1227216763024969738> {author.DisplayName} unmute {member.DisplayName}**")

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }
    }
}